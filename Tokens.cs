﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPR381Project.Tokens
{
    internal enum TokenKindEnum
    {
        Max,
        Min,
        LessEq,
        GreaterEq,
        Equal,
        Number,
        Bin, // is also >= 0 (NonNegative)
        Int, // is also >= 0 (NonNegative)
        NonNegative,
        NewLine,
    }

    internal class TokenKind
    {
        public static TokenKindEnum Max => TokenKindEnum.Max;
        public static TokenKindEnum Min => TokenKindEnum.Min;
        public static TokenKindEnum Less => TokenKindEnum.LessEq;
        public static TokenKindEnum Greater => TokenKindEnum.GreaterEq;
        public static TokenKindEnum Equal => TokenKindEnum.Equal;
        public static TokenKindEnum Number => TokenKindEnum.Number;
        public static TokenKindEnum Bin => TokenKindEnum.Bin;
        public static TokenKindEnum Int => TokenKindEnum.Int;
        public static TokenKindEnum NonNegative => TokenKindEnum.NonNegative;
        public static TokenKindEnum NewLine => TokenKindEnum.NewLine;
    }
    internal class Token
    {
        public TokenKindEnum Kind { get; set; }
        public string Value { get; set; }

        private static Token BuildToken(TokenKindEnum kind, string value)
        {
            var token = new Token();
            token.Kind = kind;
            token.Value = value;

            return token;
        }

        public static Token? BuildTokenProblemKind(string kind)
        {
            switch (kind)
            {
                case "max":
                   return BuildToken(TokenKindEnum.Max, kind);
                case "min":
                    return BuildToken(TokenKindEnum.Min, kind);
                default:
                    return null;
            }
        }

        public static Token? BuildTokenNumber(string num)
        {
            try
            {
                double.Parse(num);
            }
            catch (Exception)
            {
                return null;
            }
            
            return BuildToken(TokenKindEnum.Number, num);
        }

        public static Token BuildTokenNewLine()
        {
            return Token.BuildToken(TokenKindEnum.NewLine, "\n");
        }

        public static Token? BuildTokenSign(string sign)
        {
            return sign switch
            {
                "<=" => BuildToken(TokenKindEnum.LessEq, sign),
                ">=" => BuildToken(TokenKindEnum.GreaterEq, sign),
                "=" => BuildToken(TokenKindEnum.Equal, sign),
                _ => null
            };
        }

        public static Token? BuildTokenRestriction(string restriction)
        {
            return restriction switch
            {
                ">=0" => BuildToken(TokenKindEnum.NonNegative, restriction),
                "bin" => BuildToken(TokenKindEnum.Bin, restriction),
                "int" => BuildToken(TokenKindEnum.Int, restriction),
                _ => null
            };
        }
    }
}
