# 《나는 AI다》 Unity 코드 리뷰 v1.1

> 리뷰 일자: 2026-09-11
> 대상: `Assets/Scripts` 전체(생성 코드 `InputActions.cs` 제외), `Assets/Prefabs` 5종, `Assets/Scenes` 3종, `ProjectSettings`·`Packages/manifest.json`
> 환경: Unity 6000.5.9f1 / Photon Fusion 2.1.1 Stable / fusion-physics addon(git) / Input System 1.20 / URP 17.6
> 판단 기준: **1인 개발 · 모바일(Play Store / App Store) 출시 · 버그 없는 출시**. AAA 기준 아님.
> 개정: v1.1 (2026-09-11) — 질문 답변(§1), 결정사항 갱신(§2), 해결 코드 추가(§3·§4)

---

## 0. 결론 요약

- 현재 코드량(스크립트 약 25개)에 비해 구조는 대체로 단순하고, 과한 추상화는 거의 없음. **방향 자체는 맞음.**
- 핵심 문제는 한 가지로 수렴: **네트워크 객체의 생명주기(생성 → 씬 전환 → 퇴장 → 셧다운)가 씬마다 흩어진 컴포넌트에 나뉘어 있음.** 지금은 "로비 → 방 → 게임" 한 방향만 돌기 때문에 동작하지만, 재시작 투표(로비 복귀)·게임 중 퇴장·실패 재시도가 들어가는 순간 깨지는 구조.
- 출시 관점에서 코드 외적으로 빠진 항목(패키지명, 화면 방향, 프레임, Safe Area, 스토어 요구사항)이 있음. 지금 설정만 해두면 되는 것들.
- **v1.1:** "호스트가 나가도 게임 계속" 요구사항 확정 → 현재 Host Mode로는 불가. **토폴로지(D1) 재결정 필요, Shared Mode 제안.** 세션 흐름 버그(P0-1·2·3·6)는 토폴로지와 무관하게 먼저 수정 가능.

### 우선순위 정의

| 등급 | 의미 | 개수 |
|---|---|---|
| **P0** | 이미 버그이거나, 흐름이 조금만 바뀌어도 반드시 터지는 문제 | 6 |
| **P1** | 출시 전 반드시 해결 (코드 12 · 프로젝트 설정/스토어 6) | 18 |
| **P2** | 유지보수성 정리. 리팩토링 때 같이 처리하면 비용이 낮음 | 14 |
| **확인 필요** | 코드만으로 단정 불가. 테스트로 확인할 항목 | 8 |

---

## 1. 질문 답변 (v1.1 추가)

### Q1. `RemoveRunner()`에 Destroy 코드가 있는데 작동하지 않는가?

**결론: `RemoveRunner()` 자체는 동작함. 문제는 ① 실패 경로에서 호출되지 않는다는 점, ② 안의 `Destroy`가 중복이라는 점.**

| 경로 | `RemoveRunner()` 호출 | 결과 |
|---|---|---|
| 방 나가기 버튼 (`RoomUI`) | O | 정상 |
| 메뉴 나가기 버튼 (`InGamePausedPopup`) | O | 정상 |
| 빠른 매칭: 공개방 없음 → 방 생성으로 전환 | **X** | 실패한 러너가 `Runner`에 남은 채 `CreateRunner()` 재호출 |
| 방 생성: 코드 충돌 재시도 | **X** | 동일 |
| 코드 참가 실패 후 재시도 | **X** | 동일 |

- Fusion 공식 문서: *"You can only use a NetworkRunner once. Once that NetworkRunner disconnects from a game session or fails to connect it should be destroyed, and a new Network Runner instance should be created."*
- `CreateRunner()`는 `Runner != null`이면 기존 러너를 반환함.
  - 실패한 러너의 GameObject가 **아직 파괴되기 전**이면 → 이미 사용된 러너로 `StartGame`을 다시 호출.
  - **이미 파괴된 뒤**면 → Unity null 비교로 새 러너가 생성되어 우연히 동작.
  - 즉 결과가 `await` 재개 타이밍에 따라 달라짐.
- `RemoveRunner()` 내부: 시그니처가 `Shutdown(bool destroyGameObject = true, ...)`이라 `Shutdown()`이 GameObject까지 파괴함. 뒤따르는 `Destroy(runner.gameObject)`는 중복이고, 파괴 완료 후 재개되면 `MissingReferenceException` 가능.
- **해결 코드:** §3 P0-1

### Q2. 일시정지 메뉴에도 Shutdown이 필요한가? (설정/나가기 용도, 네트워크는 유지)

**결론: 필요 없음. 현재 코드도 메뉴를 열 때 Shutdown하지 않음.** 리뷰에서 지적한 것은 메뉴 스크립트가 `OnShutdown` 이벤트를 **구독**하는 부분.

| 동작 | 현재 코드 | 판단 |
|---|---|---|
| 메뉴 열기/닫기 | Player 입력맵만 끄고 켬. 네트워크 유지 | 의도대로 동작 |
| "나가기" 버튼 | `RemoveRunner()` → 내 세션 종료 | **필요한 Shutdown** (나가기 = 내 러너 종료) |
| `OnShutdown` 구독 → 로비 씬 로드 | `InGamePausedPopup`이 담당, 해제 안 함 | **문제** (P0-2) |

- 해결 방향: 메뉴는 "나가기 요청"만 함. **세션 종료 후 로비 이동은 `SessionManager` 한 곳**에서 처리. 메뉴·`PlayerDataSpawner`의 `OnShutdown` 구독 제거.
- 기획 확인: 메뉴가 열려 있는 동안 이동 입력이 꺼져 캐릭터가 멈춤 → 게임 중 메뉴를 열면 무방비. 의도라면 유지.
- 참고: Host Mode에서는 **호스트가 "나가기"를 누르면 전원 세션 종료** → Q4와 연결.
- **해결 코드:** §3 P0-2

### Q3. PlayerDataObject는 파괴되지 않고 Room·Game 씬 사이에서 유지하려는 설계였음

**결론: 설계 의도(씬 전환 동안 유지)는 맞음. 리뷰에서도 유지 대상으로 분류함(§7).** 문제는 유지가 아니라 **플레이어가 실제로 나갔을 때의 정리**와 **UI 갱신 방식**.

| 상황 | 현재 동작 | 판단 |
|---|---|---|
| Room → Game 전환 | DDOL 부모 아래 유지 | 의도대로 동작 |
| Room에서 플레이어 퇴장 | `PlayerDataSpawner`가 Despawn | 정상 |
| **Game에서 플레이어 퇴장** | `PlayerDataSpawner`가 GameScene에 없음 → Despawn 안 됨 | 나간 플레이어 데이터가 남음 |
| **Game 진입** | `InGameManager`가 `SetPlayerObject`를 아바타로 덮어씀 | 이후 퇴장 처리 시 잘못된 객체 참조 |
| **Game → Room 복귀** | 슬롯 UI는 `Spawned()` 시점에만 추가 | 복귀한 Room의 목록이 비어 있음 |

- 해결 방향: 객체는 지금처럼 유지. **스폰/디스폰 담당(`PlayerDataSpawner`)만 씬 독립(DDOL)으로 옮기고**, 슬롯 UI는 활성화될 때 등록부(`PlayerDataManager`)에서 현재 목록을 읽음.
- "나갔다가 **재접속**했을 때 같은 데이터를 이어받기"까지 의도했다면 별도 기능. Fusion 2.1 `StartGameArgs.PlayerUniqueId`(같은 ID로 재접속 시 같은 `PlayerRef` 사용)로 구현 가능. 이번 범위에서는 **Future 제안**.
- **해결 코드:** §3 P0-4 / P0-5, §4 P1-1

### Q4. 호스트가 나가면 다른 플레이어에게 넘기고 게임이 계속되어야 함

**결론: 현재 Host Mode 구조로는 불가능.** 현재 설정(`HostMigration.EnableAutoUpdate: false`)에서는 호스트 퇴장·강제종료·백그라운드 전환 시 모든 클라이언트 세션이 종료됨.

- 이 요구사항이 확정이면 **토폴로지(D1)를 다시 결정해야 함.**
- 제안: **Shared Mode 전환.** 선택지 비교와 변경 범위는 §2 D1.

### Q5. 방 코드 4자리 유지, 추후 변경 예정

- 반영함. 자리수를 `SessionManager.RoomCodeLength` **상수 하나**로 모아서 코드 생성·입력 검증·입력 필드 글자 수 제한이 같이 따라가도록 함.
- 나중에 바꿀 때는 상수 1곳만 수정.
- **해결 코드:** §3 P0-6

### Q6. 현재 Unity 코드는 로비 방식

- D2 **확정: 로비 방식** (빠른 매칭 / 방 생성 / 코드 참가).
- 기획서 §2·§14는 원작 방식(코드 비우면 호스트)이므로 기획서 갱신 필요 (요청 시 진행).

### 목표 프레임: 기기에 맞춰 설정

- D4 **확정: 기기 주사율 기준.**
- **해결 코드:** §4 P1-S3

### 출시 시점: 내년 이후

- 스토어 요구사항(P1-S6)은 매년 바뀜 → **출시 6개월 전 재확인** 항목으로 우선순위 하향. 문서의 수치는 2026-09 기준 참고값.
- 패키지명(P1-S1)은 **Play Console에 첫 테스트 빌드를 올리기 전**까지만 정하면 됨.

---

## 2. 리팩토링 전에 결정할 사항

| # | 항목 | 상태 (v1.1) |
|---|---|---|
| D1 | 네트워크 토폴로지 | **재결정 필요** — "호스트가 나가도 게임 계속" 요구사항(Q4) |
| D2 | 입장 방식 | **확정** — 로비 방식, 방 코드 4자리(추후 변경) |
| D3 | 단위계 | **미정** |
| D4 | 목표 프레임 | **확정** — 기기 주사율 기준 |

### D1. 네트워크 토폴로지 — 재결정 필요

- 현재 코드: **Host Mode** (`GameMode.Host` / `GameMode.Client`) + `NetworkRigidbody`(fusion-physics).
- `NetworkRigidbody`는 **Shared Mode에서 스스로 Despawn됨** (`NetworkRigidbody.cs` `SetupPhysicsBody()`: "should not be used in shared mode").
- 요구사항: 호스트가 나가면 다른 플레이어에게 넘기고 게임 지속 (확정).

#### 선택지 비교

| 항목 | A. Host Mode + Host Migration | B. Shared Mode (**제안**) |
|---|---|---|
| 호스트(마스터) 퇴장 시 | 모든 피어가 러너를 **종료 후 새 러너로 재시작**. 새 호스트가 스냅샷에서 NetworkObject를 **직접 다시 스폰·복원** | 마스터 클라이언트 **자동 재지정**. 재접속 없음. 각 플레이어는 자기 캐릭터를 계속 조작 |
| 잃는 상태 | 마지막 스냅샷 이후 변경분 (공식 권장 주기 30초, 현재 설정 10초). 공식 문서상 RPC·입력·씬 오브젝트·물리 상태·PlayerRef 미보존 | 마스터 소유 객체(봇·타이머)가 퇴장 감지까지 일시 정지. 상태 손실 없음 |
| 게임 중 체감 | 전원 수 초 끊김 + 위치 되감기 | 봇·자기장만 잠깐 멈춤 |
| 구현량 | 객체 종류마다 복원 코드 (아바타·데이터·봇·아이템·발전기·탄환), PlayerRef 재매핑, 씬 오브젝트 처리 | 스폰 주체·권한 체크 변경, 봇·게임 상태 객체에 `Is Master Client Object` 설정 |
| 현재 코드 재사용 | 높음 (NetworkRigidbody, 호스트 판정 유지) | 중간 (이동: NetworkRigidbody → NetworkTransform + 로컬 Rigidbody2D) |
| 판정 권위 | 호스트 1명 → 공정성·치팅 방지 유리. Lag Compensation 사용 가능 | 각자 자기 캐릭터가 권위 → 치팅에 약함. 공격은 기획서 §8.1대로 **피격자 권한이 확정** |
| 네트워크 틱 | 최대 256Hz (현재 60Hz) | **최대 32Hz** (Fusion 2.1 Shared 제한) |
| 모바일 조작감 | 클라이언트 예측 + 재시뮬레이션 | 자기 캐릭터는 로컬 권위 → 입력 지연 없음, 재시뮬레이션 CPU 부담 없음 |
| 모바일 백그라운드 | 호스트가 홈 버튼 → **전원** 마이그레이션 | 마스터가 홈 버튼 → 마스터만 교체 |
| 기획서 §3.1 | 불일치 | 원래 제안과 일치 |

#### 제안: B (Shared Mode)

- 모바일에서 "호스트 이탈"은 퇴장 버튼보다 **앱 백그라운드 전환**으로 훨씬 자주 발생함. A는 그때마다 전원 재접속·상태 복원이 일어나고, 복원 코드는 게임 기능이 늘어날수록 같이 늘어남 → 1인 개발에서 버그 표면이 가장 큼.
- 방 코드로 모이는 캐주얼 파티 게임 성격상, B의 치팅 취약성은 MVP에서 수용 가능하다고 판단. (공개 빠른 매칭 비중이 커지면 재검토)
- 32Hz 틱은 탑다운 이동 게임에 충분. 단, 근접 판정(범위 45px)의 지연 오차는 **플레이테스트로 확인 필요**.
- 게임플레이 코드가 이동·스태미나뿐인 **지금이 전환 비용이 가장 낮은 시점**.

#### B 선택 시 변경 범위

| 대상 | 현재 (Host) | Shared |
|---|---|---|
| `SessionManager` | `GameMode.Host` / `GameMode.Client` | `GameMode.Shared`. 코드 참가 시 없는 방이 새로 생기지 않도록 `EnableClientSessionCreation` 동작 **테스트 확인 필요** |
| 방장 판정 (시작 버튼, 모드 선택) | `runner.IsServer` | `runner.IsSharedModeMasterClient` |
| 씬 로드 | 호스트 | 마스터 클라이언트 |
| `PlayerDataObject` 스폰 | 호스트가 `PlayerJoined`에서 스폰 | **각 클라이언트가 자기 것 스폰**. `Destroy When State Authority Leaves` 체크 → 퇴장 시 자동 정리 (P0-4 자연 해결) |
| 닉네임 설정 | `RPC_SetNickname` | 본인이 State Authority → `Spawned()`에서 직접 대입 (RPC 불필요) |
| 아바타 스폰 | 호스트가 전원 스폰 | 각 클라이언트가 `OnSceneLoadDone`에서 자기 아바타만 스폰 |
| 이동 | `GetInput` + `NetworkRigidbody` | `HasStateAuthority`일 때 로컬 입력으로 이동 + `NetworkTransform`. 다른 사람 캐릭터의 Rigidbody2D는 Kinematic |
| 봇 / 자기장 / 타이머 | 호스트 | 마스터 클라이언트 소유 (`Is Master Client Object`) |
| 사망 판정 | 호스트 | 공격자 → 피격자 State Authority에 RPC → 피격자가 거리·각도·방어 상태 검증 후 확정 |

#### A 선택 시 필요한 것

- `NetworkProjectConfig`에서 Host Migration 활성화.
- `NetworkEvents.OnHostMigration`: `runner.Shutdown(shutdownReason: ShutdownReason.HostMigration)` → 새 러너로 `StartGame` (`HostMigrationToken`, `HostMigrationResume` 전달).
- `HostMigrationResume`: `runner.GetResumeSnapshotNetworkObjects()` 순회 → `Spawn` + `CopyStateFrom`.
- 킬·발전기 완료 같은 중요 이벤트 직후 `runner.PushHostMigrationSnapshot()`으로 손실 구간 축소.
- 세션 종료 처리(§3 P0-2 코드)에서 `ShutdownReason.HostMigration`은 로비로 보내지 않도록 예외 처리.

> **D1이 정해져야 §3 P0-4·P0-5, §4 P1-1·P1-4·P1-5의 스폰/입력 코드가 확정됨.**
> 아래 해결 코드 중 스폰·입력 관련은 **현재 구조(Host) 기준 최소 수정**이며, B 선택 시 위 표 기준으로 대체됨.
> 세션 흐름 코드(P0-1·P0-2·P0-3·P0-6, P1-7·P1-10)는 **토폴로지와 무관**하게 그대로 사용 가능.

### D2. 입장 방식 — 확정

- 로비 방식 (빠른 매칭 / 방 생성 / 코드 참가), 방 코드 4자리.
- 자리수는 상수 1곳에서 관리 (§3 P0-6).
- 기획서 §2·§14 갱신 필요.

### D3. 단위계 — 수치 환산 규칙 미확정

- 기획서 수치(이동 2.2, 대시 4.2 등)는 원작 기준 **"60fps에서 프레임당 px"** (`dt * 60` 방식).
- 현재 코드는 `moveSpeed = 2.2f`를 **초당 유닛**으로 사용 중이고, 캐릭터 콜라이더 반지름은 `0.5`(원작 12).
- 원작 수치를 그대로 쓰면 체감 속도가 원작과 크게 다름. 환산 규칙 확정 필요.
- **제안(가정: 1 unit = 24px, 반지름 12 → 0.5):**
  - 이동 = 2.2 × 60 ÷ 24 = **5.5 u/s**
  - 대시 = 4.2 × 60 ÷ 24 = **10.5 u/s**
  - 유령 = 3.5 × 60 ÷ 24 = **8.75 u/s**
  - 거리 계열(공격 범위 45 → 1.875, 월드 2400×1800 → 100×75)도 동일 규칙 적용
- 이 규칙은 모든 전투·시야·자기장 수치에 영향이 있으므로 **엔티티 설정값(SO) 정리 전에 확정**해야 함.

### D4. 목표 프레임 — 확정: 기기 주사율 기준

- 해결 코드: §4 P1-S3.
- 네트워크 틱(Host 60Hz / Shared 최대 32Hz)과 렌더 프레임은 별개. 120Hz 기기에서도 네트워크 시뮬레이션 비용은 같고, 렌더링·배터리·발열만 늘어남.

---

## 3. P0 — 확실한 버그 / 곧 터질 버그

### P0-1. 실패한 NetworkRunner를 재사용함

- **위치:** `SessionManager.MatchQuick()` L32~39, `CreateRoom()` 재시도 루프 L44~65, `NetworkManager.CreateRunner()` L25~26
- **문제:**
  - `StartGame()`이 실패하면 Fusion은 해당 러너를 셧다운함. **NetworkRunner는 한 번만 사용 가능.**
  - 그런데 `CreateRunner()`는 `Runner != null`이면 기존 러너를 그대로 반환함.
  - 빠른 매칭은 "공개방 없음 → 실패 → 방 생성" 경로를 탐. **방이 하나도 없을 때(첫 플레이어) 항상 이 경로.**
  - 코드 충돌 재시도(`GameIdAlreadyExists`)도 같은 러너로 재시도함.
- **왜 지금 동작하는 것처럼 보일 수 있는가:** 러너 GameObject의 파괴가 프레임 끝에 일어나므로, `await` 이후 재개 시점이 파괴 이후면 Unity null 비교로 새 러너가 만들어짐. **타이밍 의존.**
- **최소 수정:** `StartGame` 결과가 실패면 반드시 `await NetworkManager.Instance.RemoveRunner()` 후 재시도. `Runner` 참조도 즉시 null 처리.

**해결 코드** — `NetworkManager.cs` (Q1)

```csharp
public NetworkRunner CreateRunner()
{
    // 셧다운되지 않은 러너만 재사용. 한 번 사용된 러너는 버림
    if (Runner != null && !Runner.IsShutdown)
        return Runner;

    if (Runner != null)
        Destroy(Runner.gameObject);

    Runner = Instantiate(runnerPrefab, transform);
    Runner.AddCallbacks(Events);
    return Runner;
}

public async Task RemoveRunner()
{
    var runner = Runner;
    Runner = null;

    if (runner == null) return;          // Fusion이 이미 파괴함 (Unity null 비교)

    if (runner.IsShutdown)
        Destroy(runner.gameObject);      // 실패로 셧다운됐지만 GameObject가 남아 있는 경우
    else
        await runner.Shutdown();         // destroyGameObject 기본값 true → 별도 Destroy 불필요
}
```

`SessionManager.cs` — `StartGame()`의 마지막 `return await runner.StartGame(...)` 부분만 변경

```csharp
var result = await runner.StartGame(new StartGameArgs
{
    // 기존 필드 그대로
});

if (result.Ok)
    _inSession = true;                            // P0-2에서 추가하는 필드
else
    await NetworkManager.Instance.RemoveRunner(); // 실패한 러너 정리 → 다음 시도는 새 러너

return result;
```

- `MatchQuick()`, `CreateRoom()` 재시도 루프는 수정 불필요 (다음 `StartGame`이 새 러너를 만듦).
- P2-8(중복 Destroy)도 이 코드로 함께 해결.
- 테스트: 방이 없는 상태에서 빠른 매칭 10회 반복(C3). 실패한 러너의 `IsShutdown` 값을 1회 로그로 확인.

### P0-2. `InGamePausedPopup`의 `OnShutdown` 리스너가 해제되지 않음

- **위치:** `InGamePausedPopup.OnEnable()` L28 / `OnDisable()` L31~35
- **문제:** `NetworkEvents`는 `DontDestroyOnLoad` 객체라 씬이 바뀌어도 살아 있음. `OnDisable`에서 `RemoveListener`를 하지 않으므로 GameScene에 들어갈 때마다 리스너가 **누적**됨.
- **영향:** 게임 2판째부터 셧다운 시 `SceneManager.LoadScene(Lobby)`가 2회 이상 호출. 파괴된 컴포넌트를 참조하는 리스너가 계속 남음.
- **최소 수정:** `OnDisable`에 `RemoveListener(OnShutdown)` 추가. (구조적으로는 P1-1에서 셧다운 처리 자체를 한 곳으로 모음)

**해결 방향 (Q2):** 세션 종료 → 로비 이동은 `SessionManager` 한 곳에서만 처리. 메뉴·`RoomUI`는 "나가기 요청"만 함.

**해결 코드** — `SessionManager.cs` (추가분)

```csharp
using System.Threading.Tasks;
using Player;                           // PlayerDataManager
using UnityEngine.SceneManagement;

// ── 필드/프로퍼티 ──
public ShutdownReason? LastEndReason { get; private set; }
private bool _inSession;                // StartGame 성공 후 true (P0-1 코드)

// ── 구독: NetworkManager.Awake에서 Events가 만들어진 뒤여야 하므로 Start에서 ──
private void Start()
{
    if (Instance != this) return;       // 로비 씬 재진입 시 생긴 중복 인스턴스는 무시
    NetworkManager.Instance.Events.OnShutdown.AddListener(OnShutdown);
}

protected override void OnDestroy()
{
    if (Instance == this && NetworkManager.Instance != null)
        NetworkManager.Instance.Events.OnShutdown.RemoveListener(OnShutdown);
    base.OnDestroy();
}

// ── 나가기: 메뉴·RoomUI에서 호출 ──
public async Task LeaveSession()
{
    await NetworkManager.Instance.RemoveRunner();   // → OnShutdown에서 로비 이동

    if (_inSession)                                 // OnShutdown이 오지 않은 예외 상황 대비
        GoToLobby(ShutdownReason.Ok);
}

public void ConsumeEndReason() => LastEndReason = null;

private void OnShutdown(NetworkRunner runner, ShutdownReason reason)
{
    if (!_inSession) return;    // 로비에서의 접속 실패는 LobbyUI가 StartGameResult로 처리
    GoToLobby(reason);
}

private void GoToLobby(ShutdownReason reason)
{
    _inSession = false;
    LastEndReason = reason;
    PlayerDataManager.Instance.Clear();     // P1-1에서 추가
    SceneManager.LoadScene(SceneName.Lobby);
}
```

`InGamePausedPopup.cs` — `OnShutdown` 구독·메서드 삭제, 나가기만 변경

```csharp
private void OnEnable()
{
    exitButton.onClick.AddListener(OnClickExitButton);
    continueButton.onClick.AddListener(OnClickContinueButton);
}

private async void OnClickExitButton()
{
    exitButton.interactable = false;
    try
    {
        await SessionManager.Instance.LeaveSession();
    }
    catch (System.Exception e)
    {
        Debug.LogException(e);
        exitButton.interactable = true;
    }
}
```

- `RoomUI.OnLeaveButtonClick()`도 같은 방식으로 `SessionManager.Instance.LeaveSession()` 호출.
- `PlayerDataSpawner`의 `OnShutDown` 구독·메서드 삭제.
- 메뉴를 여는 동작(입력맵 on/off)은 그대로 둠 → 네트워크는 계속 유지됨.

### P0-3. 게임 시작 후에도 세션이 열려 있음 → 게임 중 난입

- **위치:** `RoomUI.OnStartButtonClick()` L54~66
- **문제:** 게임 씬 로드 시 `SessionInfo.IsOpen` / `IsVisible`을 닫지 않음.
- **영향:**
  - 다른 유저가 **빠른 매칭으로 진행 중인 게임에 난입** 가능.
  - 난입자는 GameScene에서 `InGameManager`가 아바타는 스폰해 주지만, `PlayerDataSpawner`는 RoomScene에만 있으므로 **`PlayerDataObject`(닉네임)가 없는 플레이어**가 됨.
- **최소 수정:** 게임 시작 시 `_runner.SessionInfo.IsOpen = false;` (+ 공개방이면 `IsVisible = false`). 로비 복귀 시 다시 열기.

**해결 코드** — `SessionManager.cs` (추가), `RoomUI.cs` (한 줄)

```csharp
// SessionManager.cs
public void SetJoinable(bool joinable)
{
    var info = RoomInfo;                    // P1-10에서 null 안전하게 변경
    if (info == null) return;

    info.IsOpen    = joinable;
    info.IsVisible = joinable && !IsPrivate;
}
```

```csharp
// RoomUI.OnStartButtonClick()
SessionManager.Instance.SetJoinable(false);   // 게임 중 입장 차단
_runner.LoadScene(SceneRef.FromIndex(sceneIndex));
```

- 재시작 투표로 방에 돌아올 때 `SetJoinable(true)`.
- `SessionInfo.IsOpen` / `IsVisible`은 `{ get; set; }` (Fusion API 문서 확인).
- Shared Mode(D1-B)에서는 마스터 클라이언트가 호출.

### P0-4. GameScene에서 퇴장한 플레이어의 `PlayerDataObject`가 정리되지 않음 + PlayerObject 덮어쓰기

> 씬 전환 동안 객체를 유지하는 설계 자체는 맞음 (Q3). 문제는 **퇴장 시 정리 담당이 RoomScene에만 있다는 점.**

- **위치:** `PlayerDataSpawner.OnPlayerLeft()` L41~46, `InGameManager.SpawnPlayer()` L68
- **문제:**
  1. `PlayerDataObject`의 Despawn은 RoomScene의 `PlayerDataSpawner`만 담당함. **GameScene에서 나간 플레이어의 데이터 객체는 아무도 Despawn하지 않음** (DDOL 부모 아래 계속 존재).
  2. `runner.SetPlayerObject()`를 Room(데이터 객체)과 Game(아바타) 두 곳에서 호출함 → Game 진입 후 PlayerObject가 아바타로 **덮어써짐**. 이후 Room으로 돌아오면 `TryGetPlayerObject`가 이미 Despawn된 아바타를 가리킴.
- **영향:** 재시작 투표로 방에 돌아왔을 때 유령 슬롯, 퇴장 처리 실패.
- **최소 수정:** `SetPlayerObject`는 **데이터 객체 한 곳만** 사용. 아바타는 `InGameManager`의 딕셔너리로만 관리. 데이터 객체 스폰/디스폰 담당을 씬 독립적인 곳으로 이동(P1-1).

**해결 코드 (Host 기준, Q3)** — `PlayerDataSpawner`를 씬 독립으로 이동

- 컴포넌트를 RoomScene에서 제거하고 **LobbyScene의 `SessionManager` GameObject(DDOL)에 부착**. (`SessionManager` 중복 제거 시 같이 파괴되므로 별도 싱글톤 불필요)
- `playerNetworkData` 프리팹 참조는 인스펙터에서 다시 연결.

```csharp
public class PlayerDataSpawner : MonoBehaviour
{
    [Header("Network")]
    [SerializeField] private NetworkObject playerNetworkData;

    private NetworkEvents _networkEvents;

    private void Start()
    {
        _networkEvents = NetworkManager.Instance.Events;
        _networkEvents.PlayerJoined.AddListener(OnPlayerJoined);
        _networkEvents.PlayerLeft.AddListener(OnPlayerLeft);
    }

    private void OnDestroy()
    {
        if (_networkEvents == null) return;
        _networkEvents.PlayerJoined.RemoveListener(OnPlayerJoined);
        _networkEvents.PlayerLeft.RemoveListener(OnPlayerLeft);
    }

    private void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer) return;
        if (runner.TryGetPlayerObject(player, out _)) return;   // 멱등: 이미 있으면 스폰 안 함

        var obj = runner.Spawn(playerNetworkData, inputAuthority: player);
        runner.SetPlayerObject(player, obj);                    // PlayerObject = 데이터 객체 전용
    }

    private void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer) return;
        if (runner.TryGetPlayerObject(player, out var obj))
            runner.Despawn(obj);                                // 씬과 무관하게 정리
    }
}
```

- 로비에서부터 구독하므로 호스트 자신의 `PlayerJoined`도 놓치지 않음 (C1 해소).
- 로비 씬에서 스폰된 데이터 객체도 `Spawned()`에서 곧바로 `SessionManager` 밑으로 옮겨지므로 씬 전환 후에도 유지됨 (현재 설계 그대로).
- D1-B(Shared)면 이 클래스 대신 **각 클라이언트가 자기 데이터 객체를 스폰**하고 `Destroy When State Authority Leaves`로 정리.

### P0-5. `InGameManager` 중복 스폰 가능

- **위치:** `InGameManager.SpawnPlayer()` L62~69
- **문제:** `runner.Spawn()`을 먼저 호출하고 나서 `_players.TryAdd()`를 함. 같은 플레이어에 대해 두 번 호출되면(`OnSceneLoadDone`의 `ActivePlayers` 순회 + `PlayerJoined`가 겹치는 경우) **두 번째 아바타가 스폰되고 딕셔너리에는 안 들어가 추적 불가.**
- **최소 수정:** `if (_players.ContainsKey(player)) return;`을 `Spawn` 이전에 둠.

**해결 코드 (Host 기준)** — `InGameManager.cs`

```csharp
private void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
{
    if (!runner.IsServer) return;
    if (!_players.Remove(player, out var obj)) return;
    runner.Despawn(obj);                     // 아바타만 정리. 데이터 객체는 PlayerDataSpawner 담당
}

private void SpawnPlayer(NetworkRunner runner, PlayerRef player)
{
    if (!runner.IsServer) return;
    if (_players.ContainsKey(player)) return;   // 멱등: OnSceneLoadDone + PlayerJoined 중복 방지

    var obj = runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, player);
    _players.Add(player, obj);
    // runner.SetPlayerObject 호출 삭제 (P0-4)
}
```

- 게임 → 방 복귀 흐름을 만들 때는 `LoadScene` 전에 `_players`의 아바타를 전부 `Despawn`하는 메서드 추가.

### P0-6. 빈 방 코드로 "참가"를 누르면 아무 공개방에 들어감

- **위치:** `LobbyUI.OnJoinButtonClick()` L87~104 → `SessionManager.JoinRoom()`
- **문제:** Fusion은 `GameMode.Client` + 빈 `SessionName`을 **랜덤 입장**으로 처리함. 입력 검증이 없음.
- **추가:** `RoomCodeInputField`의 Content Type이 Standard, 글자 수 제한 0 (씬 설정 확인 결과).
- **최소 수정:** 참가 전에 "숫자 4자리(또는 D2 확정 자리수)" 검증. 입력 필드는 Integer Number + Character Limit 설정.

**해결 코드 (Q5)** — `SessionManager.cs`, `LobbyUI.cs`

```csharp
// SessionManager.cs
public const int RoomCodeLength = 4;    // 자리수 변경 시 이 값만 수정

public static bool IsValidRoomCode(string code)
{
    if (code == null || code.Length != RoomCodeLength) return false;
    foreach (char c in code)
        if (c < '0' || c > '9') return false;   // char.IsDigit은 전각·아랍 숫자도 통과시키므로 사용 안 함
    return true;
}

// CreateRoom() 내부
var code = RandomCodeGenerator.GenerateNumbers(RoomCodeLength);

public async Task<StartGameResult> JoinRoom(string roomCode, int sceneIndex)
{
    if (!IsValidRoomCode(roomCode)) return null;   // 빈 코드 = 랜덤 입장 방지 (최종 방어선)
    return await StartGame(GameMode.Client, roomCode, sceneIndex);
}
```

```csharp
// LobbyUI.cs
private string RoomCode => roomCodeInputField.text.Trim();

private void Awake()
{
    // 입력 필드 제한을 상수와 동기화 (인스펙터 설정 대신)
    roomCodeInputField.characterValidation = TMP_InputField.CharacterValidation.Digit;
    roomCodeInputField.keyboardType        = TouchScreenKeyboardType.NumberPad;
    roomCodeInputField.characterLimit      = SessionManager.RoomCodeLength;
}

private async void OnJoinButtonClick()
{
    if (!SessionManager.IsValidRoomCode(RoomCode))
    {
        ShowMessage($"방 코드 {SessionManager.RoomCodeLength}자리를 입력해 주세요.");  // 안내 UI (신규, P1-7)
        return;
    }
    // 이하 기존 코드 (result null 체크는 result?. 로 통일)
}
```

---

## 4. P1 — 출시 전 반드시 해결

### 4.1 구조

#### P1-1. 네트워크 생명주기 책임을 한 곳으로 모으기 (리팩토링 핵심)

현재 책임 분포:

| 이벤트 | 처리하는 곳 | 문제 |
|---|---|---|
| 데이터 객체 스폰 | `PlayerDataSpawner` (RoomScene) | Room 씬에 있을 때만 동작 |
| 데이터 객체 디스폰 | `PlayerDataSpawner` (RoomScene) | GameScene 퇴장 누락 (P0-4) |
| 아바타 스폰/디스폰 | `InGameManager` (GameScene) | 중복 가드 없음 (P0-5) |
| 셧다운 → 로비 이동 | `PlayerDataSpawner` + `InGamePausedPopup` | 중복, 리스너 누수 (P0-2), LobbyScene/RoomUI에서 끊기면 처리자 다름 |
| 슬롯 UI 추가/삭제 | `PlayerDataObject.Spawned()`가 UI 싱글톤 직접 호출 | 씬 재진입 시 목록 복원 불가 |

**제안 구조 (새 시스템 추가 없이 기존 클래스 책임만 재배치):**

```
[DDOL] NetworkManager        러너 생성/제거 (실패 시 정리 보장)
[DDOL] SessionManager        StartGame/Leave + OnShutdown 단일 처리
                             → 로비 씬 로드 + 종료 사유 전달
                             → 게임 시작 시 세션 닫기 / 로비 복귀 시 열기
[DDOL] PlayerDataManager     PlayerDataObject 등록부 (현재 그대로)
                             + OnAdded / OnRemoved / OnChanged 이벤트
[DDOL] PlayerDataSpawner     RoomScene → 세션 레벨로 이동 (호스트에서만 동작)
                             PlayerJoined: 데이터 객체 스폰 + SetPlayerObject
                             PlayerLeft: 데이터 객체 디스폰 (씬 무관)
[Room] PlayerSlotUIManager   OnEnable: 현재 등록부로 목록 초기화 + 이벤트 구독
[Game] InGameManager         OnSceneLoadDone: ActivePlayers 순회(ContainsKey 가드)
                             PlayerLeft: 아바타만 정리
```

- 핵심 원칙 2개:
  1. **네트워크 이벤트 처리는 "멱등(여러 번 호출돼도 결과 같음)"하게.**
  2. **UI는 "구독 시점에 현재 상태로 초기화 + 이후 이벤트 반영".** 이벤트만 믿으면 씬 재진입 시 누락됨.
- `PlayerDataObject` → `PlayerSlotUIManager` 직접 호출이 사라지므로, 네트워크 레이어가 UI를 모르게 됨 (프로젝트 지침의 "게임플레이/UI 분리"와도 일치).

**해결 코드** — `PlayerDataManager.cs` (등록부 이벤트 추가)

```csharp
using System;
using System.Collections.Generic;
using Fusion;

namespace Player
{
    public class PlayerDataManager : SingletonPersistent<PlayerDataManager>
    {
        public event Action<PlayerRef, PlayerDataObject> OnAdded;
        public event Action<PlayerRef> OnRemoved;
        public event Action<PlayerRef, PlayerDataObject> OnChanged;

        private readonly Dictionary<PlayerRef, PlayerDataObject> _players = new();
        public IReadOnlyDictionary<PlayerRef, PlayerDataObject> Players => _players;

        public void Add(PlayerRef player, PlayerDataObject data)
        {
            if (_players.TryAdd(player, data))
                OnAdded?.Invoke(player, data);
        }

        public void Remove(PlayerRef player)
        {
            if (_players.Remove(player))
                OnRemoved?.Invoke(player);
        }

        public void NotifyChanged(PlayerRef player)
        {
            if (_players.TryGetValue(player, out var data))
                OnChanged?.Invoke(player, data);
        }

        public bool Get(PlayerRef player, out PlayerDataObject data) => _players.TryGetValue(player, out data);

        public void Clear() => _players.Clear();
    }
}
```

`PlayerDataObject.cs` — UI 직접 호출 제거

```csharp
public override void Spawned()
{
    transform.SetParent(SessionManager.Instance.transform);   // 씬 전환 동안 유지 (현재 설계 유지)

    var player = Object.InputAuthority;
    gameObject.name = $"PlayerData_{player.AsIndex:0000}";
    PlayerDataManager.Instance.Add(player, this);

    if (HasInputAuthority)
        RPC_SetNickname(GameManager.Instance.Nickname ?? string.Empty);
}

public override void Despawned(NetworkRunner runner, bool hasState)
{
    if (PlayerDataManager.Instance != null)
        PlayerDataManager.Instance.Remove(Object.InputAuthority);
}

private void OnNicknameChanged()
{
    PlayerDataManager.Instance.NotifyChanged(Object.InputAuthority);
}
```

`PlayerSlotUIManager.cs` — 활성화 시 현재 목록으로 초기화 + 이벤트 구독

```csharp
private void OnEnable()
{
    var registry = PlayerDataManager.Instance;

    foreach (var pair in registry.Players)                 // 씬 재진입 시 목록 복원
        Add(pair.Key, pair.Value.Nickname.ToString());

    registry.OnAdded   += HandleAdded;
    registry.OnRemoved += Remove;
    registry.OnChanged += HandleChanged;
}

private void OnDisable()
{
    var registry = PlayerDataManager.Instance;
    if (registry == null) return;

    registry.OnAdded   -= HandleAdded;
    registry.OnRemoved -= Remove;
    registry.OnChanged -= HandleChanged;
}

private void HandleAdded(PlayerRef player, PlayerDataObject data)   => Add(player, data.Nickname.ToString());
private void HandleChanged(PlayerRef player, PlayerDataObject data) => Set(player, data.Nickname.ToString());
```

- 기존 `Add` / `Remove` / `Set` 메서드는 그대로 재사용.
- 네트워크 객체(`PlayerDataObject`)가 UI를 모르게 됨 → GameScene에서 `PlayerSlotUIManager.Instance`를 매번 탐색하던 문제(P2-2)도 사라짐.

#### P1-2. 게임 상태(`GameState`)가 네트워크 동기화되지 않음

- **위치:** `GameManager.cs`
- **문제:** 기획서 §2는 LOBBY/PLAYING/END 상태가 **네트워크로 동기화**돼야 함. 현재 `GameState`는 로컬 enum이고, `SetPhase()`는 어디서도 호출되지 않음. 이름은 `GameManager`지만 실제로는 닉네임 보관만 함.
- **제안:** 게임 씬에 `NetworkBehaviour` 하나(예: 매치 상태 객체)를 두고 `[Networked] State`, `[Networked] TickTimer` (180초 타이머), 모드를 보관. 자기장·승패 판정도 이 객체의 `FixedUpdateNetwork`(호스트)에서 처리.
- `GameManager`는 닉네임 등 **로컬 설정 보관용**으로 역할을 좁히거나 이름 변경.
- 기획서 enum(3개)과 코드 enum(5개: `GameStart`, `RestartVote` 추가)도 맞춰야 함. `RestartVote`는 END 상태 내부 기능이라 별도 상태는 불필요해 보임 → **제안**.

#### P1-3. ScriptableObject에 런타임 상태가 들어 있음 + 상태가 이중으로 존재

- **위치:** `EntityStatsData.cs`, `EntityNetworkData.cs`, `PlayerDataObject.EntityData`, `PlayerController`
- **문제:**
  - `EntityStatsData`(SO)에 `hp`, `isAlive`, `position`, `velocity` 같은 **런타임 값**이 있음. SO 에셋은 모든 인스턴스가 공유하고, 에디터에서는 값 변경이 에셋에 남음 → 봇 30개가 같은 SO를 쓰면 상태가 섞임.
  - 스태미나·탈진이 `EntityNetworkData`(PlayerDataObject)와 `PlayerController`의 `[Networked]` 두 곳에 존재 → **진실의 원천이 둘.**
  - `PlayerDataObject`는 플레이어 전용인데, 기획서 §5는 "플레이어와 봇이 같은 엔티티 구조". 봇에는 `PlayerDataObject`가 없으므로 엔티티 상태를 여기에 두면 봇과 공유 불가.
  - `Hp`는 기획서에 없음(전부 즉사). `Id`는 `PlayerRef`/`NetworkId`와 중복. → YAGNI.
- **제안 역할 분리:**

| 대상 | 담는 것 | 예 |
|---|---|---|
| SO (설정) | **변하지 않는 값만** | 이동/대시 속도, 최대 스태미나, 소모/회복량, 탈진 시간, 반지름 |
| 아바타 NetworkBehaviour (플레이어·봇 공용) | 게임플레이 런타임 상태 `[Networked]` | Stamina, IsExhausted, IsAlive, Action, 쿨타임 TickTimer |
| PlayerDataObject | 세션 레벨 플레이어 정보 | 닉네임, 방장 여부, 재시작 투표 |

- `PlayerController`의 `[SerializeField]` 수치들을 SO로 옮기면 봇도 같은 SO 참조로 해결됨.

### 4.2 게임플레이 코드

#### P1-4. `PlayerController` 스태미나/이동 로직

- **L29 `Stamina = maxStamina;`** — `Spawned()`에서 권한 확인 없이 `[Networked]` 값을 씀. 늦게 들어온 클라이언트에서 받은 값을 100으로 덮었다가 다음 스냅샷에서 튐. → `if (HasStateAuthority)`.
- **L37 `if (!GetInput(...)) return;`** — 입력이 안 들어온 틱에는 탈진 타이머·스태미나 회복이 **정지**함. 입력 처리와 시간 경과 처리를 분리해야 함.
- **L22 `ExhaustedTimer` float 감산** — Fusion 관용 방식은 `TickTimer`. 틱 레이트와 무관하고 롤백/재시뮬레이션에 안전함.
- **L59 대시** — 방향 입력이 0이어도 대시 버튼만 누르면 스태미나가 소모됨. 원작 규칙 확인 필요. (**확인 필요**)
- **L76 `direction.normalized`** — 입력 크기와 무관하게 항상 최고 속도. 터치 조이스틱의 아날로그 입력이 무의미해짐. 치팅 방지 목적이라면 `Vector2.ClampMagnitude(direction, 1f)`가 아날로그와 방어를 둘 다 만족함. (기획서 §15.1 "8방향 아날로그" 기준)
- **L79 `transform.rotation`** — 루트를 회전시키면 이후 붙을 닉네임 라벨·게이지 등 자식도 전부 회전함. 기획서의 "눈 2개가 angle 방향으로 회전"은 **시각 자식만 회전**하거나 `[Networked] Angle`로 처리하는 편이 안전.
- **L62~71** — 대시 중 스태미나 0 도달 틱에서 `isDashing = false`로 바꾸지만 해당 틱은 이미 대시 소모가 적용된 상태. 체감 영향은 1틱이라 경미.

**해결 코드 (Host 기준)** — `PlayerController.cs` 스태미나 부분

```csharp
[Networked] public float Stamina { get; private set; }
[Networked] private TickTimer ExhaustTimer { get; set; }

public bool IsExhausted => !ExhaustTimer.ExpiredOrNotRunning(Runner);   // UI용

public override void Spawned()
{
    _rb = GetComponent<Rigidbody2D>();
    if (HasStateAuthority)
        Stamina = maxStamina;
    // 카메라 설정은 기존과 동일
}

public override void FixedUpdateNetwork()
{
    GetInput(out NetworkInputData input);          // 입력이 없는 틱은 default(이동 0, 대시 off)로 진행
    float dt = Runner.DeltaTime;

    Vector2 dir = Vector2.ClampMagnitude(input.MoveDirection, 1f);   // 아날로그 유지 + 과대 입력 차단
    bool wantsDash = input.Buttons.IsSet(InputButton.Dash) && dir.sqrMagnitude > 0f; // C2 확인 후 이동 조건 유지/삭제
    bool isDashing = wantsDash && !IsExhausted && Stamina > 0f;

    if (isDashing)
    {
        Stamina = Mathf.Max(0f, Stamina - dashDrain * dt);
        if (Stamina <= 0f)
            ExhaustTimer = TickTimer.CreateFromSeconds(Runner, exhaustDuration);
    }
    else if (!IsExhausted)
    {
        Stamina = Mathf.Min(maxStamina, Stamina + staminaRecovery * dt);
    }

    Move(dir, isDashing);
}
```

- 입력이 없는 틱에도 탈진 타이머·회복이 진행됨.
- `TickTimer`는 틱 기준이라 재시뮬레이션·틱 레이트 변경에 안전.
- D1-B(Shared)면 `GetInput` 대신 `if (!HasStateAuthority) return;` 후 로컬 입력을 직접 사용. 스태미나 로직 자체는 동일.
- 기존 `Move()`에서는 스태미나 처리 코드를 제거하고 이동·회전만 담당.
- `InputButton`, `input.Buttons`는 P1-5 코드 적용 기준.

#### P1-5. 입력 수집 방식 — 버튼 입력이 유실될 수 있음

- **위치:** `PlayerInput.OnMove()` L40~46, `OnInput()` L35~38
- **문제:**
  - 콜백에서 구조체를 **통째로 새로 만들어** 대입 → 다른 필드(`IsDashing` 등)가 매번 초기화됨. 지금은 `IsDashing`이 어디서도 설정되지 않음.
  - 공격/대시 같은 버튼을 콜백 기반 bool로 넣으면, **틱과 틱 사이에 눌렀다 뗀 입력은 유실됨.** (60Hz 틱, 모바일 프레임 드랍 시 더 자주 발생)
- **제안 (Fusion 표준 방식):**
  - `NetworkInputData`에 `NetworkButtons Buttons` 사용.
  - 로컬에서는 버튼을 **누적(OR)** 하고 `OnInput`에서 전송한 뒤 초기화.
  - 호스트는 `Buttons.GetPressed(PreviousButtons)`로 "이번 틱에 눌림"을 판정.
- 공격/방어/사격이 들어오기 **전에** 구조를 잡아두는 것이 비용이 가장 낮음.

**해결 코드 (Host 기준)** — 버튼 누적 방식

```csharp
// NetworkInputData.cs
public enum InputButton
{
    Dash      = 0,
    Primary   = 1,   // 공격 / 상호작용
    Secondary = 2,   // 방어 / 손전등 / 아이템
}

public struct NetworkInputData : INetworkInput
{
    public Vector2        MoveDirection;
    public NetworkButtons Buttons;
}
```

```csharp
// PlayerInput.cs
private Vector2        _move;
private NetworkButtons _pressedSinceLastTick;   // 틱 사이에 눌렀다 뗀 입력도 보존

public void OnMove(InputAction.CallbackContext context) => _move = context.ReadValue<Vector2>();

// 액션 추가 후 생성되는 콜백 예시 (Primary 액션을 .inputactions에 추가해야 존재함)
public void OnPrimary(InputAction.CallbackContext context)
{
    if (context.performed) _pressedSinceLastTick.Set(InputButton.Primary, true);
}

private void OnInput(NetworkRunner runner, NetworkInput input)
{
    var buttons = _pressedSinceLastTick;
    // 누르고 있는 동안 유지되는 버튼(대시, 발전기 수리)은 현재 상태를 그대로 넣음
    // buttons.Set(InputButton.Dash, InputManager.Instance.Actions.Player.Dash.IsPressed());

    input.Set(new NetworkInputData { MoveDirection = _move, Buttons = buttons });
    _pressedSinceLastTick = default;
}
```

```csharp
// PlayerController.cs (공격 추가 시)
[Networked] private NetworkButtons PreviousButtons { get; set; }

// FixedUpdateNetwork 내부
var pressed = input.Buttons.GetPressed(PreviousButtons);
if (pressed.IsSet(InputButton.Primary)) { /* 공격 시도 */ }
PreviousButtons = input.Buttons;
```

- `NetworkButtons.Set<T>` / `IsSet<T>` / `GetPressed`는 Fusion 2.1.1 `Fusion.Runtime.xml`에서 확인.
- 주석 처리된 줄은 `.inputactions`에 해당 액션을 추가해야 컴파일됨.

#### P1-6. 모바일 입력 미구현 (계획 대비 확인)

- 현재 바인딩은 키보드(PC 스킴)만 있음. 모바일 조작은 기획서 §15에 정의됨.
- **제안 (새 시스템 없이):** Input System의 `OnScreenStick`을 `<Gamepad>/leftStick`에 매핑하고, 기존 `Move` 액션에 게임패드 바인딩만 추가 → `PlayerInput` 코드 수정 없이 동작. 버튼도 `OnScreenButton` 동일 방식.
- **iOS에는 뒤로가기 버튼이 없음.** 현재 일시정지는 ESC(안드로이드 뒤로가기와 매핑됨)만 있으므로, 화면 버튼 필요.
- `ProjectSettings` `activeInputHandler: 2`(Both) → **Input System Only**로 변경 권장. 구 Input Manager 불필요.

### 4.3 세션 / 모바일 환경

#### P1-7. 셧다운·연결 끊김 사유를 사용자에게 알려주지 않음

- 현재는 어떤 이유든 로비 씬으로 이동만 함. 모바일은 네트워크 전환(Wi-Fi ↔ LTE), 백그라운드 전환이 잦음.
- 최소 요구: `ShutdownReason`별 메시지 ("방장이 나갔습니다", "연결이 끊겼습니다", "방이 가득 찼습니다", "방을 찾을 수 없습니다").
- `LobbyUI`의 실패 처리도 `Debug.LogError`만 있고 사용자 표시 없음 (L61, L83, L102 주석 "오류 UI 표시 등").

**해결 코드** — 종료 사유 메시지 + 로비에서 표시

```csharp
// Network/ShutdownMessage.cs (신규)
using Fusion;

namespace Network
{
    public static class ShutdownMessage
    {
        public static string Get(ShutdownReason reason) => reason switch
        {
            ShutdownReason.GameNotFound       => "방을 찾을 수 없습니다.",
            ShutdownReason.GameIsFull         => "방이 가득 찼습니다.",
            ShutdownReason.GameClosed         => "이미 게임이 시작된 방입니다.",
            ShutdownReason.PhotonCloudTimeout => "서버에 연결할 수 없습니다.",
            ShutdownReason.ConnectionTimeout  => "연결 시간이 초과되었습니다.",
            _                                 => "연결이 종료되었습니다.",
        };
    }
}
```

```csharp
// LobbyUI.Start()
var reason = SessionManager.Instance.LastEndReason;   // P0-2에서 기록
if (reason.HasValue && reason.Value != ShutdownReason.Ok)
    ShowMessage(ShutdownMessage.Get(reason.Value));
SessionManager.Instance.ConsumeEndReason();
```

- `ShowMessage`는 신규 UI 필요. MVP는 **로비 캔버스에 TMP_Text 1개 + 3초 후 숨김**이면 충분 (팝업 시스템 불필요).
- 방장 퇴장·네트워크 끊김 시 실제로 어떤 `ShutdownReason`이 오는지는 로그로 확인 후 문구 추가.

#### P1-8. `async void` 핸들러에 예외 처리 없음

- **위치:** `LobbyUI` L45/L66/L87, `RoomUI` L49, `InGamePausedPopup` L42
- **문제:** `await` 중 예외가 나면 로그만 남고 **UI가 `interactable = false` 상태로 영원히 멈춤.** 모바일에서는 재시작 외에 복구 방법이 없음.
- **최소 수정:** `try/catch/finally`에서 UI 복구. LobbyUI의 핸들러 3개는 공통 메서드 하나로 합치면 이 처리를 한 번만 쓰면 됨 (P2-5).

**해결 코드** — `LobbyUI.cs` 핸들러 3개 통합 (P2-5 포함)

```csharp
private async void OnQuickMatchButtonClick() =>
    await RunSessionTask(sceneIndex => SessionManager.Instance.MatchQuick(sceneIndex));

private async void OnCreateButtonClick() =>
    await RunSessionTask(sceneIndex => SessionManager.Instance.CreateRoom(sceneIndex));

private async void OnJoinButtonClick()
{
    if (!SessionManager.IsValidRoomCode(RoomCode)) { /* P0-6 안내 */ return; }
    await RunSessionTask(sceneIndex => SessionManager.Instance.JoinRoom(RoomCode, sceneIndex));
}

private async Task RunSessionTask(Func<int, Task<StartGameResult>> startSession)
{
    SetUIInteractable(false);
    GameManager.Instance.Nickname = NicknameText;
    PlayerPrefs.SetString(PrefKeys.Nickname, NicknameText);        // P1-11: 닉네임 저장

    try
    {
        var result = await startSession(SceneName.GetIndex(SceneName.Room));
        if (result is { Ok: true }) return;                       // 성공 시 Fusion이 Room 씬 로드

        ShowMessage(result == null
            ? "방 생성에 실패했습니다. 다시 시도해 주세요."
            : ShutdownMessage.Get(result.ShutdownReason));
    }
    catch (Exception e)
    {
        Debug.LogException(e);
        ShowMessage("알 수 없는 오류가 발생했습니다.");
    }

    SetUIInteractable(true);                                      // 실패·예외 시 항상 복구
}
```

- `using System;`, `using System.Threading.Tasks;`, `using Fusion;` 필요.

#### P1-9. 앱 백그라운드 전환 처리 없음

- **Host Mode(현재):** 호스트가 홈 버튼을 누르면 전원 세션 종료 → Q4 요구사항 위반. iOS는 백그라운드 진입 후 수 초 내 소켓이 끊김.
- **Shared Mode(D1-B):** 마스터가 백그라운드로 가면 서버가 퇴장을 감지할 때까지 봇·자기장이 멈췄다가 새 마스터로 넘어감.
- 최소 요구 (토폴로지 무관):
  - `OnApplicationPause(true)` 시 이동 입력 초기화 (조이스틱을 누른 채로 나가면 계속 이동하는 문제 방지).
  - 복귀 시 러너 상태 확인 → 끊겼으면 로비로 + 안내 (P0-2, P1-7 코드가 처리).
- D1-B 선택 시 시도할 것 (**확인 필요**): 마스터가 `OnApplicationPause(true)`를 받으면 `runner.SetMasterClient(다른 플레이어)`로 먼저 넘겨 멈춤 구간을 줄임. 일시정지 직전에 요청이 전송되는지는 실기기 테스트 필요.

```csharp
// PlayerInput.cs (Host 기준)
private void OnApplicationPause(bool paused)
{
    if (paused) _move = Vector2.zero;
}
```

#### P1-10. `SessionManager` 프로퍼티 null/키 없음 처리

- **L14** `PlayerCount => RoomInfo.PlayerCount` — null 체크 없음.
- **L17** `(bool)RoomInfo?.Properties[...]` — `RoomInfo`가 null이면 null → bool 변환에서 예외. 키가 없으면 `KeyNotFoundException`.
- **L19** `NetworkManager.Instance?.Runner?.SessionInfo` — `?.`는 Unity의 파괴된 객체를 null로 인식하지 않음 (P2-3).
- 현재는 성공 직후에만 호출돼서 드러나지 않을 뿐, 끊김 직후 UI 갱신 등에서 터질 수 있음. → `TryGetValue` 사용.

**해결 코드** — `SessionManager.cs`

```csharp
public int PlayerCount => RoomInfo?.PlayerCount ?? 0;   // SessionInfo는 일반 C# 클래스라 ?. 사용 가능

public string RoomCode =>
    RoomInfo != null && RoomInfo.Properties.TryGetValue(PrefKeys.RoomCode, out var p) ? (string)p : string.Empty;

public bool IsPrivate =>
    RoomInfo != null && RoomInfo.Properties.TryGetValue(PrefKeys.IsPrivate, out var p) && (bool)p;

private SessionInfo RoomInfo
{
    get
    {
        // NetworkManager·NetworkRunner는 UnityEngine.Object → ?. 대신 명시적 null 비교
        var manager = NetworkManager.Instance;
        var runner  = manager != null ? manager.Runner : null;
        return runner != null && runner.IsRunning ? runner.SessionInfo : null;
    }
}
```

#### P1-11. 닉네임 처리

- **저장 안 됨:** `LobbyUI.Start()`에서 `PlayerPrefs`를 읽기만 하고 쓰지 않음. `GameManager.Nickname`의 저장 코드는 주석 처리됨.
- **검증 없음:** 기획서 §14 규칙(빈 칸 → `player(N)`, 중복 → `이름(1)`) 미구현. 중복 판정은 모든 이름을 아는 **호스트**가 `RPC_SetNickname`에서 처리해야 함.
- **null 전송 가능:** `RPC_SetNickname(GameManager.Instance?.Nickname)` L27.
- **길이:** `NetworkString<_32>`보다 긴 입력 처리 기준 없음 → 입력 필드 Character Limit 설정.
- **스토어:** 닉네임·채팅은 UGC(사용자 생성 콘텐츠)에 해당. App Store 심사 가이드라인 1.2는 UGC에 **부적절 콘텐츠 필터 · 신고 · 차단 수단**을 요구함. 채팅(기획서 Optional)을 넣는다면 출시 범위에 포함해서 계산해야 함.

#### P1-12. `RoomUI` 디버그 잔재

- **L58~62** 최소 인원 체크의 `return`이 주석 처리됨 → 1명으로 게임 시작 가능.
- **L29** `startButton.enabled = _runner.IsServer;` — 컴포넌트를 끄면 클릭은 막히지만 **버튼 모양은 그대로 보임.** 게스트 입장에서 눌리지 않는 버튼. → `interactable` 또는 `SetActive`.

### 4.4 프로젝트 설정 / 스토어 출시

#### P1-S1. 패키지명 · 회사명 미설정

- `applicationIdentifier`가 Standalone(`com.DefaultCompany.2D-URP`)만 설정, Android/iOS는 기본값. `companyName: Jason`.
- **Play Store는 한 번 올린 패키지명을 변경할 수 없음.** 첫 내부 테스트 빌드 전에 확정 필요.
- v1.1: 출시가 내년 이후라 급하지는 않음. 기준은 동일하게 "첫 업로드 전".

#### P1-S2. 화면 방향

- `defaultScreenOrientation: 4`(자동 회전) + 세로 방향 허용 상태. 게임은 가로(900×600 기준) → **가로 좌/우만 허용**으로 고정.

#### P1-S3. 프레임 — D4 확정: 기기 주사율 기준

- `targetFrameRate` 미지정 → 모바일 기본 30fps.
- 해결: 앱 시작 시 기기가 지원하는 최대 주사율로 설정. 배터리·발열을 고려해 **설정 메뉴에 상한 옵션(30 / 60 / 최대)** 을 두는 것을 제안 (메뉴가 이미 있으므로 비용 낮음).

```csharp
// Utils/FrameRateSetup.cs (신규)
using UnityEngine;

namespace Utils
{
    public static class FrameRateSetup
    {
        public const string PrefKey = "FrameRateCap";   // 0 = 기기 최대

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize() => Apply(PlayerPrefs.GetInt(PrefKey, 0));

        public static void Apply(int cap)
        {
            int deviceMax = GetDeviceMaxRefreshRate();
            QualitySettings.vSyncCount  = 0;
            Application.targetFrameRate = cap > 0 ? Mathf.Min(cap, deviceMax) : deviceMax;
        }

        private static int GetDeviceMaxRefreshRate()
        {
            double max = Screen.currentResolution.refreshRateRatio.value;
            foreach (var resolution in Screen.resolutions)
                if (resolution.refreshRateRatio.value > max)
                    max = resolution.refreshRateRatio.value;

            return max > 1d ? Mathf.RoundToInt((float)max) : 60;
        }
    }
}
```

- **확인 필요:** 120Hz 기기(Android 고주사율, iPhone ProMotion)에서 실제로 120fps가 적용되는지 실기기 확인. 기기·OS 설정(절전 모드 등)에 따라 제한될 수 있음.
- `CameraFollow` 보간을 프레임 독립으로 바꾸는 P2-7도 같이 적용해야 주사율마다 체감이 같아짐.

#### P1-S4. Safe Area

- `androidRenderOutsideSafeArea: 1` → 노치/펀치홀 영역까지 렌더링됨. 기획서 §15.8대로 UI 루트에 Safe Area 적용 필요.

#### P1-S5. 로그 / 패키지 정리

- 모바일 Scripting Define에 `FUSION_LOGLEVEL_INFO` → 릴리즈는 WARN 이상 권장.
- 게임에서 사용하지 않는 패키지가 매니페스트에 있음: `com.unity.ai.assistant`(**프리릴리즈**), `com.unity.ai.inference`, `com.unity.visualscripting`, `com.unity.collab-proxy`, `com.unity.multiplayer.center`. 빌드 크기·빌드 시간·업데이트 리스크만 늘림 → 제거 권장.
- `fusion-physics`가 git 브랜치 URL로 참조됨 → `packages-lock.json`이 커밋돼 있어야 버전이 고정됨 (현재 `.gitignore`는 제외하지 않으므로 OK, 커밋 여부만 확인).

#### P1-S6. 스토어 요구사항 체크 (2026-09 기준)

> v1.1: 출시가 내년 이후이므로 **출시 6개월 전 재확인** 항목. 아래 수치는 2026-09 기준 참고값이며 그 사이 상향될 가능성이 높음.

| 항목 | 내용 |
|---|---|
| Google Play target API | 2026-08-31부터 신규 앱/업데이트는 **Android 16 (API 36)** 타깃 필요. 현재 `AndroidTargetSdkVersion: 0`(설치된 최고 버전 자동) → 설치된 SDK가 36 이상인지 확인 |
| Google Play 테스트 | 2023-11 이후 생성된 **개인** 개발자 계정은 프로덕션 출시 전 **테스터 12명 · 14일 연속 비공개 테스트** 필요. 1인 개발 일정에 반드시 반영 |
| App Store SDK | 2026년 4월 말부터 **iOS 26 SDK(Xcode 26)** 로 빌드해야 업로드 가능 |
| 서명 키 | Android 업로드 키스토어 백업 (분실 시 복구 절차 번거로움) |
| 개인정보 | Photon 사용 → Play 데이터 보안 섹션 / App Store 개인정보 라벨에 네트워크 식별자 수집 여부 선언 |
| Photon App ID | `PhotonAppSettings.asset`이 저장소에 포함됨. **공개 저장소라면** 타인이 CCU를 사용할 수 있음 → 비공개 유지 또는 빌드 시 주입 |

---

## 5. P2 — 유지보수 정리 (리팩토링 때 같이)

#### P2-1. 네임스페이스 충돌

| 현재 | 충돌 대상 | 이미 발생한 증상 |
|---|---|---|
| `namespace Camera` | `UnityEngine.Camera` | `PlayerController` L32에서 `UnityEngine.Camera.main` 전체 경로 필요 |
| `namespace Input` | `UnityEngine.Input` | 구 Input 사용 시 모호성 |
| `namespace Player` + `class Player` | 자기 자신 | `Player.Player` |
| `class PlayerInput` | `UnityEngine.InputSystem.PlayerInput` | 같은 파일에 `using UnityEngine.InputSystem;` 있음. Add Component 메뉴에 동명 컴포넌트 2개 |
| `namespace UI` | `UnityEngine.UI` | 부분 모호성 |

- **제안:** 루트 네임스페이스 `IamAI.*` 하나로 통일 (`IamAI.Player`, `IamAI.Network` …). `GameManager`, `Singleton`은 현재 전역 네임스페이스라 함께 정리.
- 클래스 이름 변경 시 `.meta` GUID가 유지되도록 **Unity 에디터 안에서(또는 Rider의 Unity 연동으로) 변경.**

#### P2-2. `Singleton<T>`

- **L12 `FindAnyObjectByType` 지연 탐색** — 초기화 순서 버그를 숨김. 씬에 없으면 매 호출마다 전체 탐색 (`PlayerSlotUIManager.Instance?`가 GameScene에서 매번 탐색함). → 제거하고 Awake 등록만 사용.
- **L20~24 중복 시 `Destroy` 후 `return`** — 하위 클래스의 `Awake`는 계속 실행됨. 예: `InputManager` 중복이면 `new InputActions()` 생성, `NetworkManager` 중복이면 Events 프리팹 Instantiate.
- **`InputActions`는 `IDisposable`** — `InputManager.OnDestroy`에서 `Dispose()` 필요.
- **부트스트랩:** 모든 DDOL 매니저가 **LobbyScene에만 배치** → RoomScene/GameScene에서 에디터 Play 시 즉시 NRE. 1인 개발 반복 속도에 영향. → 매니저들을 프리팹 하나로 묶고, 에디터에서 LobbyScene이 아닌 씬으로 시작하면 해당 프리팹을 생성하거나 LobbyScene으로 이동하는 간단한 처리.

#### P2-3. `UnityEngine.Object`에 `?.` 사용

- `?.`/`??`는 Unity의 파괴된 객체(fake null)를 null로 판정하지 않음.
- 위치: `PlayerController` L32, `PlayerInput` L22/L32, `PauseInputHandler` L14~21, `PlayerDataObject` L24/L34/L42, `SessionManager` L19, `LobbyUI` 결과 처리.
- → `if (obj != null)` 또는 `TryGetComponent`.

#### P2-4. 일시정지 3클래스 → 입력 소유권 분산

- `PauseInputHandler`(입력) + `PauseController`(토글) + `InGamePausedPopup`(UI) — ESC 토글 하나에 3클래스이며 모두 같은 GameObject에 있음.
- Player 액션맵 Enable/Disable을 **4곳**에서 호출: `PlayerInput.Spawned/Despawned`, `PauseController`, `InGamePausedPopup.OnClickContinueButton`. `InputManager.SetGameContext/SetUIContext`는 만들어 놓고 미사용.
- **제안:** 입력 맵 전환은 `InputManager`만 담당. 일시정지는 `PauseController`(입력+토글) + `InGamePausedPopup`(UI) 2개로 충분.
- 멀티플레이라 실제로 게임이 멈추지 않음 → 이름은 "메뉴"가 더 정확 (선택).
- Q2 확인: 메뉴는 네트워크를 유지하는 설정/나가기 용도가 맞음. 나가기 이후의 씬 이동만 `SessionManager`로 이관 (§3 P0-2).

#### P2-5. `LobbyUI` 중복 핸들러

- 3개 핸들러가 "UI 잠금 → 닉네임 저장 → 씬 인덱스 → await → 성공/실패" 동일 구조. `Func<Task<StartGameResult>>`를 받는 공통 메서드 1개로 통합 (P1-8 예외 처리도 여기에 한 번).
- L102 `result.ShutdownReason`만 null 조건 연산자 없음 (나머지 두 곳과 불일치).
- **해결 코드:** §4 P1-8.

#### P2-6. 사용하지 않는 코드 (YAGNI)

| 대상 | 상태 |
|---|---|
| `Player.cs` | 빈 클래스, 어떤 프리팹에도 붙어 있지 않음 |
| `GameManager.GameState` / `SetPhase` / `OnPhaseChanged` | 호출처 없음 |
| `InputManager.SetGameContext` / `SetUIContext` | 호출처 없음 |
| `SceneName.GetScene` / `GetNameByScene` | 호출처 없음, 단순 래핑 |
| `RandomCodeGenerator` 공개 메서드 7개 | `GenerateNumbers`만 사용 (111줄 → 10줄 내외) |
| `PlayerDataManager.Remove(out)` / `Set` | `Set`은 같은 객체를 다시 넣는 무의미한 호출(`OnNicknameChanged` L41) |
| `EntityNetworkData.Hp` / `Id` | 기획서에 없음 / 중복 식별자 |
| `using Photon.Client.StructWrapping;` (`RoomUI` L5) 외 미사용 using 다수 | SDK 업데이트 시 컴파일 에러 원인 |

#### P2-7. `CameraFollow` 프레임 의존 보간

- **L19** `Vector3.Lerp(a, b, smoothSpeed * Time.deltaTime)` — 30fps/60fps/120fps 기기마다 따라가는 느낌이 다름.
- → `1f - Mathf.Exp(-smoothSpeed * Time.deltaTime)`로 변경 (한 줄).

#### P2-8. `NetworkManager.RemoveRunner()` 중복 파괴

- **L38~39** `await runner.Shutdown();` 후 `Destroy(runner.gameObject);`
- Fusion 2의 `Shutdown()`은 기본 인자(`destroyGameObject: true`)로 러너 GameObject까지 파괴함. 이미 파괴된 뒤라면 `runner.gameObject` 접근에서 `MissingReferenceException` 가능 (재개 타이밍 의존). → 둘 중 하나만.
- **해결: §3 P0-1 코드에 포함 (Q1).**

#### P2-9. `PrefKeys` 용도 혼합

- `PlayerPrefs` 키(`Nickname`)와 세션 속성 키(`RoomCode`, `IsPrivate`)가 한 클래스에 섞임. `SessionPropertyKeys`로 분리 (이름만).

#### P2-10. 프리팹/씬 잔재

- `Player.prefab`의 `PlayerController`에 사라진 필드 `speed: 5`가 직렬화돼 남음. 씬 GameObject 이름이 옛 클래스명(`PlayerSlotSpawner`, `RoomManager`). → 프리팹/씬 재저장 + 이름 정리.
- 스폰 위치가 전원 `Vector3.zero` (`InGameManager` L66) → 겹친 Rigidbody2D가 튕겨나감. 기획서 §6.4 스폰 로직으로 교체 예정이면 그때 처리.

#### P2-11. 물리 설정

- `Physics2DSettings` 중력 `-9.81` → 탑다운이므로 `0`. (현재는 개별 Rigidbody `GravityScale 0`으로 막고 있음, 봇 프리팹에서 실수 방지)
- 레이어가 전혀 정의되지 않음 (충돌 매트릭스 전부 on). 수풀(트리거)·벽·엔티티·유령(벽 무시) 구분이 필요해지는 시점에 레이어 설계.

#### P2-12. 저장소 정리

- `.github/workflows/dotnet-desktop.yml`이 빈 파일(2바이트) → Actions 오류 알림 원인. 삭제.
- `.idea/`가 `.gitignore`에 없음.
- 루트의 `Scripts.csproj`, `*FusionMultiplay*.csproj`는 이전 asmdef/패키지 흔적 (gitignore 대상이라 저장소 영향은 없음).
- 이후 asmdef를 도입하면 `NetworkProjectConfig`의 `AssembliesToWeave`에 추가해야 `[Networked]`가 동작함 (현재는 `Assembly-CSharp`만).

#### P2-13. `SceneName.GetIndex`

- `SceneUtility.GetBuildIndexByScenePath`에 경로가 아닌 씬 이름(`"LobbyScene"`)을 전달 중. 현재 방 생성이 동작한다면 이름 매칭이 되는 것이지만, 문서상 인자는 경로. → 인덱스 상수 또는 전체 경로로 명시하는 편이 안전.

#### P2-14. 퍼포먼스 측정 포인트 (지금 최적화하지 말 것)

- `RunnerSimulatePhysics`는 클라이언트 재시뮬레이션 때마다 **2D 물리 씬 전체**를 다시 스텝함. 봇 27~32개 + 벽 콜라이더에서 저사양 Android 부하 측정 필요.
- 봇을 넣은 직후 **Profiler로 1회 측정** → 문제가 있을 때만 대응. 지금은 아무것도 바꾸지 않음.

---

## 6. 확인 필요 (테스트로 판정)

| # | 내용 | 확인 방법 |
|---|---|---|
| C1 | 호스트 자신의 `PlayerJoined`가 RoomScene의 `PlayerDataSpawner` 구독 **전에** 발생하는지 | 방 생성 직후 호스트 본인 슬롯이 항상 뜨는지 10회 반복. **P0-4 해결 코드(스포너 DDOL 이동) 적용 시 무관** |
| C2 | 방향 입력 없이 대시 버튼만 누를 때 스태미나 소모 여부 (원작 규칙) | 원작 V4.7 코드 확인 |
| C3 | 빠른 매칭 fallback(방 없음 → 생성)이 매번 성공하는지 | 방 0개 상태에서 빠른 매칭 10회 반복 (P0-1 재현) |
| C4 | StartGame 실패 후 러너의 `IsShutdown`이 `true`인지 | P0-1 코드 적용 후 실패 경로에서 로그 1회 |
| C5 | (D1-B) Shared Mode 코드 참가 시 없는 방이 새로 생성되지 않는지 | 존재하지 않는 코드로 참가 시도 → 방 생성 여부 확인, 필요 시 `EnableClientSessionCreation` 조정 |
| C6 | (D1-B) 마스터 백그라운드 시 `SetMasterClient` 선제 이양이 전송되는지 | 실기기 2대, 마스터 홈 버튼 → 봇 정지 시간 측정 |
| C7 | 120Hz 기기에서 `targetFrameRate`가 실제 적용되는지 | Android 고주사율 기기 / iPhone ProMotion 실기기 |
| C8 | 호스트 퇴장·네트워크 끊김 시 실제 전달되는 `ShutdownReason` 값 | 로그로 확인 후 `ShutdownMessage` 문구 추가 |

---

## 7. 유지할 것 (변경 불필요)

- `NetworkEvents`(UnityEvent)를 이벤트 허브로 사용 — 1인 개발 규모에 적절. 별도 이벤트 버스 불필요.
- `NetworkManager`가 세션마다 러너를 생성/제거하는 방식.
- `INetworkInput` + `FixedUpdateNetwork` + `Runner.DeltaTime` 기반 이동.
- 세션 레벨 플레이어 데이터를 아바타와 분리한 **개념** (`PlayerDataObject`) — **씬 전환 동안 유지하는 설계 그대로 유지**, 스폰/디스폰 담당만 씬 독립으로 이동 (Q3).
- `SetUIInteractable`로 중복 입력 방지.
- 방 코드 충돌 재시도.
- 빌드 설정: IL2CPP, ARM64 only, minSdk 26.

---

## 8. 과설계 경계 — 이번 리팩토링에서 하지 않을 것

| 하지 않을 것 | 이유 |
|---|---|
| DI 프레임워크 (VContainer, Zenject) | 매니저 5~6개 규모. 싱글톤 유지로 충분 |
| 범용 상태머신 프레임워크 | 게임 상태 3개, AI 상태 2개. `switch`로 충분 |
| 커스텀 이벤트 버스 / 메시지 시스템 | `NetworkEvents` + C# event로 충분 |
| UI MVVM / MVP 계층 | UI 스크립트 4개 |
| 오브젝트 풀링 | NIGHT 탄환 구현 시점에 탄환만 |
| 범용 팝업/알림 시스템 | 종료 사유 안내는 TMP_Text 1개로 충분 (P1-7) |
| 재접속 데이터 복원 (`PlayerUniqueId`) | Future (Q3) |
| 커스텀 충돌(원작 방식)으로 물리 교체 | P2-14 측정 결과가 나쁠 때만 재검토 |
| Host Migration 직접 구현 | D1-B(Shared) 선택 시 불필요. A 선택 시에만 3단계에서 구현 |
| Addressables | 에셋 규모 작음 |
| asmdef 분리 | 컴파일 시간 문제 생길 때. 도입 시 Weaver 설정 주의 (P2-12) |

---

## 9. 제안 리팩토링 순서 (승인 후 진행)

각 단계는 **독립 커밋**, 단계마다 ParrelSync 클론 2개로 "방 생성 → 참가 → 게임 시작 → 퇴장(호스트/게스트 각각)" 수동 테스트.

| 단계 | 내용 | 해결 항목 | 토폴로지 영향 |
|---|---|---|---|
| 0 | **D1(토폴로지)·D3(단위계) 결정**, 기획서 §2·§3·§14 갱신 | Q4, Q6 | — |
| 1 | 네임스페이스 통일, 미사용 코드/using 삭제, 프리팹·씬 재저장 (동작 변경 없음) | P2-1, P2-6, P2-10 | 무관 |
| 2 | 세션 흐름: 러너 실패 정리, 세션 종료 단일 처리 + 사유 표시, 방 코드 검증, 게임 시작 시 세션 닫기, LobbyUI 통합·예외 처리, 프로퍼티 null 안전 | P0-1, P0-2, P0-3, P0-6, P1-7, P1-8, P1-10, P2-5, P2-8 | 무관 |
| 3 | **토폴로지 적용** — B: Shared 전환(§2 D1 표) / A: Host Migration 구현 | Q4 | 핵심 |
| 4 | 플레이어 데이터 생명주기: 스폰 담당 씬 독립화, 등록부 이벤트, 슬롯 UI 초기화, 아바타 스폰 가드 | P0-4, P0-5, P1-1, P1-12 | 3단계 결과 기준 |
| 5 | 싱글톤/부트스트랩 정리, 입력 맵 소유권 단일화, 메뉴 스크립트 통합 | P2-2, P2-3, P2-4, P1-6(구조) | 무관 |
| 6 | 엔티티 데이터: SO는 설정값만, 스태미나 TickTimer, 입력 버튼 누적 | P1-3, P1-4, P1-5 | 3단계 결과 기준 |
| 7 | 네트워크 매치 상태 객체 뼈대 (State, 180초 타이머) | P1-2 | 3단계 결과 기준 |
| 8 | 프로젝트 설정: 프레임(기기 주사율), 방향, Safe Area, Input System Only, 패키지 제거, 로그 레벨, 카메라 보간 | P1-S2~S5, P2-7, P2-11~13 | 무관 |

- 1·2단계는 D1 결정 전에도 바로 진행 가능.
- 닉네임 규칙(P1-11), 백그라운드 처리(P1-9)는 기능 개발과 함께 진행. 패키지명(P1-S1)은 첫 Play Console 업로드 전, 스토어 요구사항(P1-S6)은 출시 6개월 전.
