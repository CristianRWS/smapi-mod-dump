using System.Collections.Generic;
using System.Linq;
using System.Text;
using ContentPatcher.Framework.Lexing;
using ContentPatcher.Framework.Lexing.LexTokens;
using FluentAssertions;
using NUnit.Framework;

namespace Pathoschild.Stardew.Tests.Mods.ContentPatcher
{
    /// <summary>Unit tests for <see cref="Lexer"/>.</summary>
    [TestFixture]
    // ReSharper disable once InconsistentNaming
    class LexerTests
    {
        /*********
        ** Unit tests
        *********/
        /// <summary>Test that <see cref="Lexer.TokeniseString"/> generates the expected low-level structure.</summary>
        /// <remarks>
        /// 
        /// </remarks>
        [TestCase(
            "", // input
            "[]", // bits
            "[]" // tokens
        )]
        [TestCase(
            "   ",
            "[   ]",
            "[   ]"
        )]
        [TestCase(
            "boop",
            "[boop]",
            "[boop]"
        )]
        [TestCase(
            "  assets/boop.png  ",
            "[  assets/boop.png  ]",
            "[  assets/boop.png  ]"
        )]
        [TestCase(
            "  inner whitespace with ~!@#$%^&*()_=[]{}':;\"',.<>/ characters",
            "[  inner whitespace with ~!@#$%^&*()_=[]{}']<InputArgSeparator::>[;\"',.<>/ characters]",
            "[  inner whitespace with ~!@#$%^&*()_=[]{}':;\"',.<>/ characters]"
        )]
        [TestCase(
            "{{token}}",
            "<StartToken:{{>[token]<EndToken:}}>",
            "<Token:token>"
        )]
        [TestCase(
            " {{  token }}   ",
            "[ ]<StartToken:{{>[  token ]<EndToken:}}>[   ]",
            "[ ]<Token:token>[   ]"
        )]
        [TestCase(
            " {{  Relationship : Abigail }}   ",
            "[ ]<StartToken:{{>[  Relationship ]<InputArgSeparator::>[ Abigail ]<EndToken:}}>[   ]",
            "[ ]<Token:Relationship input=<input:[Abigail]>>[   ]"
        )]
        [TestCase(
            " {{  token : inputArgument/{{subtoken}} | piped-token }}   ",
            "[ ]<StartToken:{{>[  token ]<InputArgSeparator::>[ inputArgument/]<StartToken:{{>[subtoken]<EndToken:}}>[ ]<TokenPipe:|>[ piped-token ]<EndToken:}}>[   ]",
            "[ ]<Token:token input=<input:[inputArgument/]<Token:subtoken>> piped=(<Token:piped-token>)>[   ]"
        )]
        [TestCase(
            " {{  token : inputArgument/{{subtoken}} | piped-token-a | pipedTokenB }}   ", // input
            "[ ]<StartToken:{{>[  token ]<InputArgSeparator::>[ inputArgument/]<StartToken:{{>[subtoken]<EndToken:}}>[ ]<TokenPipe:|>[ piped-token-a ]<TokenPipe:|>[ pipedTokenB ]<EndToken:}}>[   ]", // lexical bits
            "[ ]<Token:token input=<input:[inputArgument/]<Token:subtoken>> piped=(<Token:piped-token-a>)(<Token:pipedTokenB>)>[   ]" // lexical tokens
        )]
        public void ParseTokenisedString(string input, string expectedBits, string expectedTokens)
        {
            // act
            this.GetLexInfo(input, false, out LexBit[] bits, out ILexToken[] tokens);

            // assert
            this.GetComparableShorthand(bits).Should().Be(expectedBits);
            this.GetComparableShorthand(tokens).Should().Be(expectedTokens);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get lexical info for an input string.</summary>
        /// <param name="input">The raw text to tokenise.</param>
        /// <param name="impliedBraces">Whether we're parsing a token context (so the outer '{{' and '}}' are implied); else parse as a tokenisable string which main contain a mix of literal and {{token}} values.</param>
        /// <param name="bits">The low-level lexical character patterns.</param>
        /// <param name="tokens">The higher-level lexical tokens.</param>
        private void GetLexInfo(string input, bool impliedBraces, out LexBit[] bits, out ILexToken[] tokens)
        {
            Lexer lexer = new Lexer();
            bits = lexer.TokeniseString(input).ToArray();
            tokens = lexer.ParseBits(bits, impliedBraces).ToArray();
        }

        /// <summary>Get a comparable representation for a sequence of lexical bits for comparison in unit tests.</summary>
        /// <param name="input">The lexical bits to represent.</param>
        private string GetComparableShorthand(params LexBit[] input)
        {
            return string.Join("", input.Select(bit => bit.Type == LexBitType.Literal ? $"[{bit.Text}]" : $"<{bit.Type}:{bit.Text}>"));
        }

        /// <summary>Get a comparable representation for a sequence of lexical tokens for comparison in unit tests.</summary>
        /// <param name="input">The lexical bits to represent.</param>
        private string GetComparableShorthand(params ILexToken[] input)
        {
            return string.Join("", input.Select(bit =>
            {
                switch (bit)
                {
                    case LexTokenToken token:
                        {
                            StringBuilder str = new StringBuilder();

                            str.Append($"<{token.Type}:{token.Name}");
                            if (token.InputArg != null)
                                str.Append($" input={this.GetComparableShorthand(token.InputArg)}");
                            if (token.PipedTokens.Any())
                            {
                                str.Append(" piped=");
                                foreach (LexTokenToken pipedToken in token.PipedTokens)
                                    str.Append($"({this.GetComparableShorthand(pipedToken)})");
                            }

                            str.Append(">");
                            return str.ToString();
                        }

                    case LexTokenInputArg inputArg:
                        return $"<input:{this.GetComparableShorthand(inputArg.Parts)}>";

                    case LexTokenLiteral _:
                        return $"[{bit.Text}]";

                    default:
                        return $"<{bit.Type}:{bit.Text}>";
                }
            }));
        }
    }
}
