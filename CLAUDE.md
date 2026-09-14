# CLAUDE.md

## 응답 규칙

- 존댓말로 답한다.

### 코드 테스트 답변 시

코드를 수정한 뒤 동작 확인이 필요할 때, **"확인 부탁드립니다" 같은 막연한 요청 대신 기능 테스트 리스트를 만들어 제시한다.**

각 항목은 다음을 갖춘다:

| 요소 | 설명 |
|---|---|
| 번호 | 순서대로 실행할 수 있게 |
| 절차 | 사용자가 그대로 따라 할 수 있는 구체적 조작 |
| 기대 결과 | 통과/실패를 명확히 가를 수 있는 관찰 가능한 조건 |
| 대상 항목 | 어떤 수정(P0-1 등)을 검증하는지 |

- 회귀 테스트(기존 기능이 안 깨졌는지)도 포함한다.
- "잘 되는지 봐주세요" 처럼 판정 기준이 없는 문장은 쓰지 않는다.

## 프로젝트

- Unity 6000.5.9f1 / Photon Fusion 2.1.1 / URP 17.6 / Input System 1.20
- 1인 개발 · 모바일(Play Store / App Store) 출시 목표
- 네트워크 토폴로지: **Host Mode 확정**. Host Migration은 Future.

### 리팩토링 기준 문서

노션 「《나는 AI다》 Unity 코드 리뷰 v1.4」의 §10 리팩토링 순서를 따른다.
https://app.notion.com/p/3dab6047de2581449b18c81b7f65a7c5

- 각 단계는 독립 커밋으로 만든다.
- 단계마다 "방 생성 → 참가 → 게임 시작 → 퇴장" 수동 테스트를 거친다.

### 브랜치

- 작업 브랜치에서 개발 → 사용자가 `test` 브랜치로 머지
- `main`에 직접 푸시하지 않는다.

### 검증 제약

원격 세션에는 Unity·.NET 컴파일러가 없다. **컴파일 검증은 불가능하다.**
코드 변경 시 이 점을 명시하고, 위 규칙대로 기능 테스트 리스트를 제공한다.

다만 **Fusion API 시그니처는 직접 조회할 수 있다.** 추측으로 답하지 말고 확인할 것.

Fusion 어셈블리는 Git LFS 포인터로 저장돼 있어, 새 세션에서는 아래 절차가 필요하다:

```bash
apt-get install -y git-lfs
git lfs install --local
git lfs pull --include="Assets/Photon/Fusion/Assemblies/*.dll"
pip3 install --timeout 120 --retries 5 dnfile
```

`dnfile`로 메타데이터를 파싱해 타입·프로퍼티·메서드 시그니처를 확인한다.
`SessionProperty`는 `Fusion.Realtime.dll`, 나머지 대부분은 `Fusion.Runtime.dll`에 있다.
LFS 실파일을 받아도 LFS 필터가 포인터로 환산하므로 `git status`는 깨끗하게 유지된다.

#### 확인된 Fusion API (2026-09-14)

| 대상 | 사실 |
|---|---|
| `SessionInfo` | class (struct 아님). `?.`·`!= null` 정상 동작 |
| `SessionInfo.IsOpen` | `bool`, **public 세터 있음** → 방 닫기 가능 |
| `SessionInfo.IsVisible` | `bool`, public 세터 있음 |
| `SessionInfo.Properties` | `ReadOnlyDictionary<string, SessionProperty>` → `ContainsKey`·인덱서 사용 가능 |
| `SessionInfo.IsValid` / `PlayerCount` | `bool` / `int` |
| `SessionProperty` | class. `string`·`bool`·`int`와 양방향 `op_Implicit` |
| `NetworkRunner` | `IsShutdown`·`IsServer`·`SessionInfo`·`Shutdown`·`Spawn`·`SetPlayerObject` 모두 public |
| `ShutdownReason` | 22개 값 (Ok, GameNotFound, GameIsFull, GameClosed, ServerInRoom 등) |
