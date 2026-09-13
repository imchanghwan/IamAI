using UnityEngine;

namespace IamAI.Utils
{
    /// <summary>
    /// 무작위 문자열 코드를 생성하는 정적 유틸리티 클래스
    /// </summary>
    public static class RandomCodeGenerator
    {
        // 문자 세트 정의
        private const string Numbers = "0123456789";

        // 가독성 문자 세트 (유사 문자 0, 1 제외)
        private const string CleanNumbers = "23456789";

        #region Public API

        /// <summary>
        /// 숫자로만 구성된 무작위 코드를 생성합니다. (예: 839201)
        /// </summary>
        /// <param name="length">생성할 코드의 길이 (1 이상)</param>
        /// <param name="excludeSimilar">true일 경우 헷갈리기 쉬운 유사 숫자(0, 1)를 제외합니다.</param>
        /// <returns>생성된 무작위 숫자 문자열</returns>
        public static string GenerateNumbers(int length, bool excludeSimilar = false)
            => GenerateCore(excludeSimilar ? CleanNumbers : Numbers, length);

        #endregion

        #region Core Implementation

        private static string GenerateCore(string characterSet, int length)
        {
            if (length <= 0)
            {
                Debug.LogError($"[{nameof(RandomCodeGenerator)}] {nameof(length)}는 1 이상이어야 합니다.");
                return string.Empty;
            }

            var result = new char[length];
            int setLength = characterSet.Length;

            for (int i = 0; i < length; i++)
            {
                result[i] = characterSet[UnityEngine.Random.Range(0, setLength)];
            }

            return new string(result);
        }

        #endregion
    }
}
