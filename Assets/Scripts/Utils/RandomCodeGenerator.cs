using System;
using UnityEngine;

namespace Utils
{
    /// <summary>
    /// 무작위 문자열 코드를 생성하는 정적 유틸리티 클래스
    /// </summary>
    public static class RandomCodeGenerator
    {
        // 문자 세트 정의
        private const string UpperLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string LowerLetters = "abcdefghijklmnopqrstuvwxyz";
        private const string Numbers = "0123456789";

        // 가독성 문자 세트 (유사 문자 0, O, 1, I, l 제외)
        private const string CleanUpperLetters = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        private const string CleanLowerLetters = "abcdefghijkmnopqrstuvwxyz";
        private const string CleanNumbers = "23456789";

        #region Public API

        /// <summary>
        /// 영문 대문자로 구성된 무작위 코드를 생성합니다. (예: KXPQLMWZ)
        /// </summary>
        /// <param name="length">생성할 코드의 길이 (1 이상)</param>
        /// <param name="excludeSimilar">true일 경우 헷갈리기 쉬운 유사 문자(I, O)를 제외합니다.</param>
        /// <returns>생성된 무작위 영문 대문자 문자열</returns>
        public static string GenerateUpperLetters(int length, bool excludeSimilar = false)
            => GenerateCore(excludeSimilar ? CleanUpperLetters : UpperLetters, length);

        /// <summary>
        /// 영문 소문자로 구성된 무작위 코드를 생성합니다. (예: kxpqlmwz)
        /// </summary>
        /// <param name="length">생성할 코드의 길이 (1 이상)</param>
        /// <param name="excludeSimilar">true일 경우 헷갈리기 쉬운 유사 문자(l)를 제외합니다.</param>
        /// <returns>생성된 무작위 영문 소문자 문자열</returns>
        public static string GenerateLowerLetters(int length, bool excludeSimilar = false)
            => GenerateCore(excludeSimilar ? CleanLowerLetters : LowerLetters, length);

        /// <summary>
        /// 영문 대소문자로 구성된 무작위 코드를 생성합니다. (예: KxPqLmWz)
        /// </summary>
        /// <param name="length">생성할 코드의 길이 (1 이상)</param>
        /// <param name="excludeSimilar">true일 경우 헷갈리기 쉬운 유사 문자(I, O, l)를 제외합니다.</param>
        /// <returns>생성된 무작위 영문 대소문자 문자열</returns>
        public static string GenerateLetters(int length, bool excludeSimilar = false)
            => GenerateCore(excludeSimilar ? (CleanUpperLetters + CleanLowerLetters) : (UpperLetters + LowerLetters), length);

        /// <summary>
        /// 숫자로만 구성된 무작위 코드를 생성합니다. (예: 839201)
        /// </summary>
        /// <param name="length">생성할 코드의 길이 (1 이상)</param>
        /// <param name="excludeSimilar">true일 경우 헷갈리기 쉬운 유사 숫자(0, 1)를 제외합니다.</param>
        /// <returns>생성된 무작위 숫자 문자열</returns>
        public static string GenerateNumbers(int length, bool excludeSimilar = false)
            => GenerateCore(excludeSimilar ? CleanNumbers : Numbers, length);

        /// <summary>
        /// 영문 대문자와 숫자로 구성된 무작위 코드를 생성합니다. (예: K9X2P8L0MZ - 일반적인 게임 쿠폰/시리얼 키)
        /// </summary>
        /// <param name="length">생성할 코드의 길이 (1 이상)</param>
        /// <param name="excludeSimilar">true일 경우 헷갈리기 쉬운 유사 문자 및 숫자(0, O, 1, I)를 제외합니다.</param>
        /// <returns>생성된 무작위 영대문자+숫자 문자열</returns>
        public static string GenerateUpperAlphaNumeric(int length, bool excludeSimilar = false)
            => GenerateCore(excludeSimilar ? (CleanUpperLetters + CleanNumbers) : (UpperLetters + Numbers), length);

        /// <summary>
        /// 영문 소문자와 숫자로 구성된 무작위 코드를 생성합니다. (예: k9x2p8l0mz)
        /// </summary>
        /// <param name="length">생성할 코드의 길이 (1 이상)</param>
        /// <param name="excludeSimilar">true일 경우 헷갈리기 쉬운 유사 문자 및 숫자(0, 1, l)를 제외합니다.</param>
        /// <returns>생성된 무작위 영소문자+숫자 문자열</returns>
        public static string GenerateLowerAlphaNumeric(int length, bool excludeSimilar = false)
            => GenerateCore(excludeSimilar ? (CleanLowerLetters + CleanNumbers) : (LowerLetters + Numbers), length);

        /// <summary>
        /// 영문 대소문자와 숫자로 구성된 무작위 코드를 생성합니다. (예: K9x2P8l0mZ)
        /// </summary>
        /// <param name="length">생성할 코드의 길이 (1 이상)</param>
        /// <param name="excludeSimilar">true일 경우 헷갈리기 쉬운 유사 문자 및 숫자(0, O, 1, I, l)를 제외합니다.</param>
        /// <returns>생성된 무작위 영문 대소문자+숫자 혼합 문자열</returns>
        public static string GenerateAlphaNumeric(int length, bool excludeSimilar = false)
            => GenerateCore(excludeSimilar ? (CleanUpperLetters + CleanLowerLetters + CleanNumbers) : (UpperLetters + LowerLetters + Numbers), length);

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