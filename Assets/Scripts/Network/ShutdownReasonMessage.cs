using Fusion;

namespace IamAI.Network
{
    /// <summary>
    /// <see cref="ShutdownReason"/>을 사용자에게 보여줄 안내 문구로 변환한다.
    /// 열거형 이름을 그대로 노출하면 사용자가 원인을 알 수 없으므로 이 표를 거친다.
    /// </summary>
    public static class ShutdownReasonMessage
    {
        public const string Unknown = "알 수 없는 오류로 연결에 실패했습니다.";

        /// <summary>
        /// StartGame 결과에 대한 안내 문구. 결과가 null이면(재시도 소진 등) 기본 문구를 돌려준다.
        /// </summary>
        public static string Get(StartGameResult result)
        {
            return result == null ? Unknown : Get(result.ShutdownReason);
        }

        public static string Get(ShutdownReason reason)
        {
            switch (reason)
            {
                case ShutdownReason.Ok:
                    return "세션이 종료되었습니다.";

                // 방 찾기 / 참가
                case ShutdownReason.GameNotFound:
                    return "존재하지 않는 방 코드입니다.";
                case ShutdownReason.GameIsFull:
                    return "방이 가득 찼습니다.";
                case ShutdownReason.GameClosed:
                    return "이미 시작된 게임이라 참가할 수 없습니다.";
                case ShutdownReason.GameIdAlreadyExists:
                    return "같은 코드의 방이 이미 있습니다. 다시 시도해 주세요.";

                // 연결 / 네트워크
                case ShutdownReason.ConnectionTimeout:
                case ShutdownReason.OperationTimeout:
                case ShutdownReason.PhotonCloudTimeout:
                    return "서버 응답이 없습니다. 네트워크 상태를 확인해 주세요.";
                case ShutdownReason.ConnectionRefused:
                    return "서버가 연결을 거부했습니다.";
                case ShutdownReason.DisconnectedByPluginLogic:
                    return "서버에서 연결이 끊어졌습니다.";

                // 호스트 (D1: Host Mode — 방장이 나가면 게임 종료)
                case ShutdownReason.ServerInRoom:
                    return "방장이 나가 게임이 종료되었습니다.";
                case ShutdownReason.HostMigration:
                    return "방장이 변경되었습니다.";

                // 서버 / 설정
                case ShutdownReason.MaxCcuReached:
                    return "서버 접속자가 가득 찼습니다. 잠시 후 다시 시도해 주세요.";
                case ShutdownReason.InvalidRegion:
                    return "서버 지역 설정이 올바르지 않습니다.";
                case ShutdownReason.IncompatibleConfiguration:
                    return "앱 버전이 서버와 맞지 않습니다. 업데이트해 주세요.";

                // 인증
                case ShutdownReason.AuthenticationTicketExpired:
                    return "인증이 만료되었습니다. 다시 시도해 주세요.";
                case ShutdownReason.InvalidAuthentication:
                case ShutdownReason.CustomAuthenticationFailed:
                    return "인증에 실패했습니다.";

                // 기타
                case ShutdownReason.OperationCanceled:
                    return "요청이 취소되었습니다.";
                case ShutdownReason.InvalidArguments:
                    return "잘못된 요청입니다.";
                case ShutdownReason.AlreadyRunning:
                    return "이미 실행 중인 세션이 있습니다.";

                case ShutdownReason.Error:
                default:
                    return Unknown;
            }
        }
    }
}
