using System;
using System.Collections.Generic;

namespace GameDevWare.Dynamic.Expressions.Properties
{
	public static class Resources
	{
		public static string[] SupportedLanguages
		{
			get
			{
				return (string[])Resources.supportedLanguages.Clone();
			}
		}

		public static string CurrentLanguage
		{
			get
			{
				return Resources.supportedLanguages[Resources.currentLanguageIdx];
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				int num = Array.IndexOf<string>(Resources.supportedLanguages, value);
				if (num < 0)
				{
					throw new ArgumentException("Unsupported language '" + value + "'.", "value");
				}
				Resources.currentLanguageIdx = num;
			}
		}

		public static Dictionary<string, string> All
		{
			get
			{
				return new Dictionary<string, string>(58)
				{
					{
						"EXCEPTION_PARSER_INVALIDCHILDCOUNTOFNODE",
						Resources.EXCEPTION_PARSER_INVALIDCHILDCOUNTOFNODE
					},
					{
						"EXCEPTION_BIND_UNABLETOINVOKENONDELEG",
						Resources.EXCEPTION_BIND_UNABLETOINVOKENONDELEG
					},
					{
						"EXCEPTION_COMPIL_NOCONVERTIONBETWEENTYPES",
						Resources.EXCEPTION_COMPIL_NOCONVERTIONBETWEENTYPES
					},
					{
						"EXCEPTION_BOUNDEXPR_CANTCONVERTARG",
						Resources.EXCEPTION_BOUNDEXPR_CANTCONVERTARG
					},
					{
						"EXCEPTION_BIND_UNABLETORESOLVEMEMBERONTYPE",
						Resources.EXCEPTION_BIND_UNABLETORESOLVEMEMBERONTYPE
					},
					{
						"EXCEPTION_COMPIL_NOBINARYOPONTYPE",
						Resources.EXCEPTION_COMPIL_NOBINARYOPONTYPE
					},
					{
						"EXCEPTION_BIND_UNABLETORESOLVETYPE",
						Resources.EXCEPTION_BIND_UNABLETORESOLVETYPE
					},
					{
						"EXCEPTION_BIND_TOOMANYARGUMENTS",
						Resources.EXCEPTION_BIND_TOOMANYARGUMENTS
					},
					{
						"EXCEPTION_COMPIL_UNKNOWNEXPRTYPE",
						Resources.EXCEPTION_COMPIL_UNKNOWNEXPRTYPE
					},
					{
						"EXCEPTION_PARSER_COLONISEXPRECTED",
						Resources.EXCEPTION_PARSER_COLONISEXPRECTED
					},
					{
						"EXCEPTION_BIND_UNABLETOAPPLYNULLCONDITIONALOPERATORONTYPEREF",
						Resources.EXCEPTION_BIND_UNABLETOAPPLYNULLCONDITIONALOPERATORONTYPEREF
					},
					{
						"EXCEPTION_BIND_UNABLETOBINDCONSTRUCTOR",
						Resources.EXCEPTION_BIND_UNABLETOBINDCONSTRUCTOR
					},
					{
						"EXCEPTION_BIND_UNABLEREMAPPARAMETERSCOUNTMISMATCH",
						Resources.EXCEPTION_BIND_UNABLEREMAPPARAMETERSCOUNTMISMATCH
					},
					{
						"EXCEPTION_PARSER_TERNARYOPREQOPERAND",
						Resources.EXCEPTION_PARSER_TERNARYOPREQOPERAND
					},
					{
						"EXCEPTION_BIND_INVALIDLAMBDAARGUMENTS",
						Resources.EXCEPTION_BIND_INVALIDLAMBDAARGUMENTS
					},
					{
						"EXCEPTION_BIND_CLOSEDDELEGATETYPEISEXPECTED",
						Resources.EXCEPTION_BIND_CLOSEDDELEGATETYPEISEXPECTED
					},
					{
						"EXCEPTION_BIND_UNABLETORESOLVENAME",
						Resources.EXCEPTION_BIND_UNABLETORESOLVENAME
					},
					{
						"EXCEPTION_BIND_VALIDDELEGATETYPEISEXPECTED",
						Resources.EXCEPTION_BIND_VALIDDELEGATETYPEISEXPECTED
					},
					{
						"EXCEPTION_BOUNDEXPR_WRONGNUMPARAMS",
						Resources.EXCEPTION_BOUNDEXPR_WRONGNUMPARAMS
					},
					{
						"EXCEPTION_PARSER_OPREQUIRESOPERAND",
						Resources.EXCEPTION_PARSER_OPREQUIRESOPERAND
					},
					{
						"EXCEPTION_BIND_UNABLETOCREATEEXPRWITHPARAMS",
						Resources.EXCEPTION_BIND_UNABLETOCREATEEXPRWITHPARAMS
					},
					{
						"EXCEPTION_PARSER_BINARYOPREQOPERAND",
						Resources.EXCEPTION_PARSER_BINARYOPREQOPERAND
					},
					{
						"EXCEPTION_PARSER_UNARYOPREQOPERAND",
						Resources.EXCEPTION_PARSER_UNARYOPREQOPERAND
					},
					{
						"EXCEPTION_STRINGUTILS_UNEXPECTEDESCAPESEQ",
						Resources.EXCEPTION_STRINGUTILS_UNEXPECTEDESCAPESEQ
					},
					{
						"EXCEPTION_PARSER_UNEXPECTEDTOKENWHILEBUILDINGTREE",
						Resources.EXCEPTION_PARSER_UNEXPECTEDTOKENWHILEBUILDINGTREE
					},
					{
						"EXCEPTION_COMPIL_UNKNOWNBINARYEXPRTYPE",
						Resources.EXCEPTION_COMPIL_UNKNOWNBINARYEXPRTYPE
					},
					{
						"EXCEPTION_BIND_UNABLETOBINDDELEG",
						Resources.EXCEPTION_BIND_UNABLETOBINDDELEG
					},
					{
						"EXCEPTION_PARSER_OPREQUIRESSECONDOPERAND",
						Resources.EXCEPTION_PARSER_OPREQUIRESSECONDOPERAND
					},
					{
						"EXCEPTION_PARSER_TYPENAMEEXPECTED",
						Resources.EXCEPTION_PARSER_TYPENAMEEXPECTED
					},
					{
						"EXCEPTION_UNBOUNDEXPR_TYPESDOESNTMATCHNAMES",
						Resources.EXCEPTION_UNBOUNDEXPR_TYPESDOESNTMATCHNAMES
					},
					{
						"EXCEPTION_PARSER_INVALIDCHILDTYPESOFNODE",
						Resources.EXCEPTION_PARSER_INVALIDCHILDTYPESOFNODE
					},
					{
						"EXCEPTION_BIND_FAILEDTOBIND",
						Resources.EXCEPTION_BIND_FAILEDTOBIND
					},
					{
						"EXCEPTION_COMPIL_ONLYFUNCLAMBDASISSUPPORTED",
						Resources.EXCEPTION_COMPIL_ONLYFUNCLAMBDASISSUPPORTED
					},
					{
						"EXCEPTION_BIND_UNABLETORESOLVETYPEMULTIPLE",
						Resources.EXCEPTION_BIND_UNABLETORESOLVETYPEMULTIPLE
					},
					{
						"EXCEPTION_COMPIL_UNKNOWNUNARYEXPRTYPE",
						Resources.EXCEPTION_COMPIL_UNKNOWNUNARYEXPRTYPE
					},
					{
						"EXCEPTION_COMPIL_NOUNARYOPONTYPE",
						Resources.EXCEPTION_COMPIL_NOUNARYOPONTYPE
					},
					{
						"EXCEPTION_BOUNDEXPR_BODYRESULTDOESNTMATCHRESULTTYPE",
						Resources.EXCEPTION_BOUNDEXPR_BODYRESULTDOESNTMATCHRESULTTYPE
					},
					{
						"EXCEPTION_BIND_INVALIDCONSTANTEXPRESSION",
						Resources.EXCEPTION_BIND_INVALIDCONSTANTEXPRESSION
					},
					{
						"EXCEPTION_UNBOUNDEXPR_DUPLICATEPARAMNAME",
						Resources.EXCEPTION_UNBOUNDEXPR_DUPLICATEPARAMNAME
					},
					{
						"EXCEPTION_PARSER_UNEXPECTEDTOKEN",
						Resources.EXCEPTION_PARSER_UNEXPECTEDTOKEN
					},
					{
						"EXCEPTION_UNBOUNDEXPR_INVALIDPARAMCOUNT",
						Resources.EXCEPTION_UNBOUNDEXPR_INVALIDPARAMCOUNT
					},
					{
						"EXCEPTION_LIST_LISTISEMPTY",
						Resources.EXCEPTION_LIST_LISTISEMPTY
					},
					{
						"EXCEPTION_BOUNDEXPR_WRONGPARAMETERTYPE",
						Resources.EXCEPTION_BOUNDEXPR_WRONGPARAMETERTYPE
					},
					{
						"EXCEPTION_BIND_MISSINGATTRONNODE",
						Resources.EXCEPTION_BIND_MISSINGATTRONNODE
					},
					{
						"EXCEPTION_BIND_UNKNOWNEXPRTYPE",
						Resources.EXCEPTION_BIND_UNKNOWNEXPRTYPE
					},
					{
						"EXCEPTION_BIND_INVALIDCHARLITERAL",
						Resources.EXCEPTION_BIND_INVALIDCHARLITERAL
					},
					{
						"EXCEPTION_PARSER_EXPRESSIONISEMPTY",
						Resources.EXCEPTION_PARSER_EXPRESSIONISEMPTY
					},
					{
						"EXCEPTION_BIND_RENDERFAILED",
						Resources.EXCEPTION_BIND_RENDERFAILED
					},
					{
						"EXCEPTION_BOUNDEXPR_ARGSDOESNTMATCHPARAMS",
						Resources.EXCEPTION_BOUNDEXPR_ARGSDOESNTMATCHPARAMS
					},
					{
						"EXCEPTION_BIND_MISSINGORWRONGARGUMENT",
						Resources.EXCEPTION_BIND_MISSINGORWRONGARGUMENT
					},
					{
						"EXCEPTION_BIND_UNABLETOBINDCALL",
						Resources.EXCEPTION_BIND_UNABLETOBINDCALL
					},
					{
						"EXCEPTION_PARSER_UNEXPECTEDTOKENTYPE",
						Resources.EXCEPTION_PARSER_UNEXPECTEDTOKENTYPE
					},
					{
						"EXCEPTION_BIND_FAILEDTOBINDGENERICARGUMENTSTOTYPE",
						Resources.EXCEPTION_BIND_FAILEDTOBINDGENERICARGUMENTSTOTYPE
					},
					{
						"EXCEPTION_BIND_MISSINGMETHODPARAMETER",
						Resources.EXCEPTION_BIND_MISSINGMETHODPARAMETER
					},
					{
						"EXCEPTION_BIND_UNABLETOBINDINDEXER",
						Resources.EXCEPTION_BIND_UNABLETOBINDINDEXER
					},
					{
						"EXCEPTION_PARSER_UNEXPECTEDTOKENWHILEOTHEREXPECTED",
						Resources.EXCEPTION_PARSER_UNEXPECTEDTOKENWHILEOTHEREXPECTED
					},
					{
						"EXCEPTION_TOKENIZER_INVALIDCHARLITERAL",
						Resources.EXCEPTION_TOKENIZER_INVALIDCHARLITERAL
					},
					{
						"EXCEPTION_TOKENIZER_UNEXPECTEDSYMBOL",
						Resources.EXCEPTION_TOKENIZER_UNEXPECTEDSYMBOL
					}
				};
			}
		}

		public static string EXCEPTION_PARSER_INVALIDCHILDCOUNTOFNODE
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_PARSER_INVALIDCHILDCOUNTOFNODE";
				}
				return "An invalid children count '{1}' of node '{0}' while {2} is expected.";
			}
		}

		public static string EXCEPTION_BIND_UNABLETOINVOKENONDELEG
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_BIND_UNABLETOINVOKENONDELEG";
				}
				return "Unable to invoke non-delegate type '{0}'.";
			}
		}

		public static string EXCEPTION_COMPIL_NOCONVERTIONBETWEENTYPES
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_COMPIL_NOCONVERTIONBETWEENTYPES";
				}
				return "No conversion operation is defined from '{0}' to '{1}'.";
			}
		}

		public static string EXCEPTION_BOUNDEXPR_CANTCONVERTARG
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_BOUNDEXPR_CANTCONVERTARG";
				}
				return "Can't convert argument '{0}' ('{2}') to required type '{1}'.";
			}
		}

		public static string EXCEPTION_BIND_UNABLETORESOLVEMEMBERONTYPE
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_BIND_UNABLETORESOLVEMEMBERONTYPE";
				}
				return "Unable to find public member with name '{0}' on '{1}' type.";
			}
		}

		public static string EXCEPTION_COMPIL_NOBINARYOPONTYPE
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_COMPIL_NOBINARYOPONTYPE";
				}
				return "No binary operation '{0}' is defined on type '{1}'.";
			}
		}

		public static string EXCEPTION_BIND_UNABLETORESOLVETYPE
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_BIND_UNABLETORESOLVETYPE";
				}
				return "Unable to resolve type '{0}'.";
			}
		}

		public static string EXCEPTION_BIND_TOOMANYARGUMENTS
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_BIND_TOOMANYARGUMENTS";
				}
				return "Too many arguments. Maximum number of supported arguments '{0}'.";
			}
		}

		public static string EXCEPTION_COMPIL_UNKNOWNEXPRTYPE
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_COMPIL_UNKNOWNEXPRTYPE";
				}
				return "Unknown expression type {0}.";
			}
		}

		public static string EXCEPTION_PARSER_COLONISEXPRECTED
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_PARSER_COLONISEXPRECTED";
				}
				return "A colon ':' symbol is expected in conditional '?' expression.";
			}
		}

		public static string EXCEPTION_BIND_UNABLETOAPPLYNULLCONDITIONALOPERATORONTYPEREF
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_BIND_UNABLETOAPPLYNULLCONDITIONALOPERATORONTYPEREF";
				}
				return "Unable to apply null-conditional operator on type '{0}'.";
			}
		}

		public static string EXCEPTION_BIND_UNABLETOBINDCONSTRUCTOR
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_BIND_UNABLETOBINDCONSTRUCTOR";
				}
				return "Unable to find constructor on type '{0}' accepting specified arguments.";
			}
		}

		public static string EXCEPTION_BIND_UNABLEREMAPPARAMETERSCOUNTMISMATCH
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_BIND_UNABLEREMAPPARAMETERSCOUNTMISMATCH";
				}
				return "Unable to remap expression's parameters with lamda syntax. Parameters count mismatch.";
			}
		}

		public static string EXCEPTION_PARSER_TERNARYOPREQOPERAND
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_PARSER_TERNARYOPREQOPERAND";
				}
				return "A ternary operation requires three parameters.";
			}
		}

		public static string EXCEPTION_BIND_INVALIDLAMBDAARGUMENTS
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_BIND_INVALIDLAMBDAARGUMENTS";
				}
				return "Invalid argument types or count for lambda of type '{0}'.";
			}
		}

		public static string EXCEPTION_BIND_CLOSEDDELEGATETYPEISEXPECTED
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_BIND_CLOSEDDELEGATETYPEISEXPECTED";
				}
				return "Invalid lambda type '{0}'. A closed delegate type is expected in lambda type declaration.";
			}
		}

		public static string EXCEPTION_BIND_UNABLETORESOLVENAME
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_BIND_UNABLETORESOLVENAME";
				}
				return "Unable to resolve '{0}'. There is no formal parameter with this name.";
			}
		}

		public static string EXCEPTION_BIND_VALIDDELEGATETYPEISEXPECTED
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_BIND_VALIDDELEGATETYPEISEXPECTED";
				}
				return "Invalid lambda type '{0}'. A valid delegate type should be specified in lambda type declaration.";
			}
		}

		public static string EXCEPTION_BOUNDEXPR_WRONGNUMPARAMS
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_BOUNDEXPR_WRONGNUMPARAMS";
				}
				return "Wrong number of parameters. Make sure parameter count matches expression's signature.";
			}
		}

		public static string EXCEPTION_PARSER_OPREQUIRESOPERAND
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_PARSER_OPREQUIRESOPERAND";
				}
				return "A '{0}' operator requires an operand.";
			}
		}

		public static string EXCEPTION_BIND_UNABLETOCREATEEXPRWITHPARAMS
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_BIND_UNABLETOCREATEEXPRWITHPARAMS";
				}
				return "Unable to create '{0}' expression with these '{1}' parameters.";
			}
		}

		public static string EXCEPTION_PARSER_BINARYOPREQOPERAND
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_PARSER_BINARYOPREQOPERAND";
				}
				return "A binary operation requires two parameters.";
			}
		}

		public static string EXCEPTION_PARSER_UNARYOPREQOPERAND
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_PARSER_UNARYOPREQOPERAND";
				}
				return "An unary operation requires one parameter.";
			}
		}

		public static string EXCEPTION_STRINGUTILS_UNEXPECTEDESCAPESEQ
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_STRINGUTILS_UNEXPECTEDESCAPESEQ";
				}
				return "Unknown escape sequence '{0}'.";
			}
		}

		public static string EXCEPTION_PARSER_UNEXPECTEDTOKENWHILEBUILDINGTREE
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_PARSER_UNEXPECTEDTOKENWHILEBUILDINGTREE";
				}
				return "Unexpected parser node met '{0}' while building expression tree.";
			}
		}

		public static string EXCEPTION_COMPIL_UNKNOWNBINARYEXPRTYPE
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_COMPIL_UNKNOWNBINARYEXPRTYPE";
				}
				return "Unknown binary expression type '{0}'.";
			}
		}

		public static string EXCEPTION_BIND_UNABLETOBINDDELEG
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_BIND_UNABLETOBINDDELEG";
				}
				return "Unable to invoke delegate {0}({1}) with specified arguments.";
			}
		}

		public static string EXCEPTION_PARSER_OPREQUIRESSECONDOPERAND
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_PARSER_OPREQUIRESSECONDOPERAND";
				}
				return "A '{0}' operator requires a second operand.";
			}
		}

		public static string EXCEPTION_PARSER_TYPENAMEEXPECTED
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_PARSER_TYPENAMEEXPECTED";
				}
				return "A type name is expected.";
			}
		}

		public static string EXCEPTION_UNBOUNDEXPR_TYPESDOESNTMATCHNAMES
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_UNBOUNDEXPR_TYPESDOESNTMATCHNAMES";
				}
				return "Length of types array doesn't match length of names array.";
			}
		}

		public static string EXCEPTION_PARSER_INVALIDCHILDTYPESOFNODE
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_PARSER_INVALIDCHILDTYPESOFNODE";
				}
				return "An invalid type of children nodes '{1}' of node '{0}' while '{2}' is expected.";
			}
		}

		public static string EXCEPTION_BIND_FAILEDTOBIND
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_BIND_FAILEDTOBIND";
				}
				return "An error occured while trying to build '{0}' expression: {1}";
			}
		}

		public static string EXCEPTION_COMPIL_ONLYFUNCLAMBDASISSUPPORTED
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_COMPIL_ONLYFUNCLAMBDASISSUPPORTED";
				}
				return "Only System.Func<> lambda types are supported.";
			}
		}

		public static string EXCEPTION_BIND_UNABLETORESOLVETYPEMULTIPLE
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_BIND_UNABLETORESOLVETYPEMULTIPLE";
				}
				return "Unable to resolve type '{0}'. Can't choose from: '{1}'.";
			}
		}

		public static string EXCEPTION_COMPIL_UNKNOWNUNARYEXPRTYPE
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_COMPIL_UNKNOWNUNARYEXPRTYPE";
				}
				return "Unknown unary expression type '{0}'.";
			}
		}

		public static string EXCEPTION_COMPIL_NOUNARYOPONTYPE
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_COMPIL_NOUNARYOPONTYPE";
				}
				return "No unary operation '{0}' is defined on type '{1}'.";
			}
		}

		public static string EXCEPTION_BOUNDEXPR_BODYRESULTDOESNTMATCHRESULTTYPE
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_BOUNDEXPR_BODYRESULTDOESNTMATCHRESULTTYPE";
				}
				return "Body's result type doesn't match expression's result type.";
			}
		}

		public static string EXCEPTION_BIND_INVALIDCONSTANTEXPRESSION
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_BIND_INVALIDCONSTANTEXPRESSION";
				}
				return "Can't convert constant of type '{0}' to literal representation.";
			}
		}

		public static string EXCEPTION_UNBOUNDEXPR_DUPLICATEPARAMNAME
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_UNBOUNDEXPR_DUPLICATEPARAMNAME";
				}
				return "Duplicate parameter name '{0}'.";
			}
		}

		public static string EXCEPTION_PARSER_UNEXPECTEDTOKEN
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_PARSER_UNEXPECTEDTOKEN";
				}
				return "Unexpected token '{0}' in current context.";
			}
		}

		public static string EXCEPTION_UNBOUNDEXPR_INVALIDPARAMCOUNT
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_UNBOUNDEXPR_INVALIDPARAMCOUNT";
				}
				return "Invalid parameters count.";
			}
		}

		public static string EXCEPTION_LIST_LISTISEMPTY
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_LIST_LISTISEMPTY";
				}
				return "List is empty.";
			}
		}

		public static string EXCEPTION_BOUNDEXPR_WRONGPARAMETERTYPE
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_BOUNDEXPR_WRONGPARAMETERTYPE";
				}
				return "One of parameters has invalid type. Make sure parameter types matches expression's signature.";
			}
		}

		public static string EXCEPTION_BIND_MISSINGATTRONNODE
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_BIND_MISSINGATTRONNODE";
				}
				return "Missing or wrong '{0}' attribute on one of expression nodes.";
			}
		}

		public static string EXCEPTION_BIND_UNKNOWNEXPRTYPE
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_BIND_UNKNOWNEXPRTYPE";
				}
				return "Unknown expression type '{0}'.";
			}
		}

		public static string EXCEPTION_BIND_INVALIDCHARLITERAL
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_BIND_INVALIDCHARLITERAL";
				}
				return "Invalid char literal '{0}'. It should be one character length.";
			}
		}

		public static string EXCEPTION_PARSER_EXPRESSIONISEMPTY
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_PARSER_EXPRESSIONISEMPTY";
				}
				return "Expression is empty";
			}
		}

		public static string EXCEPTION_BIND_RENDERFAILED
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_BIND_RENDERFAILED";
				}
				return "An error occured while trying to render '{0}' expression: {1}";
			}
		}

		public static string EXCEPTION_BOUNDEXPR_ARGSDOESNTMATCHPARAMS
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_BOUNDEXPR_ARGSDOESNTMATCHPARAMS";
				}
				return "Count of passed arguments doesn't match parameters count.";
			}
		}

		public static string EXCEPTION_BIND_MISSINGORWRONGARGUMENT
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_BIND_MISSINGORWRONGARGUMENT";
				}
				return "Missing or wrong '{0}' argument.";
			}
		}

		public static string EXCEPTION_BIND_UNABLETOBINDCALL
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_BIND_UNABLETOBINDCALL";
				}
				return "Unable to find method '{0}' on type '{1}' accepting {2} arguments.";
			}
		}

		public static string EXCEPTION_PARSER_UNEXPECTEDTOKENTYPE
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_PARSER_UNEXPECTEDTOKENTYPE";
				}
				return "Unexpected token type '{0}'.";
			}
		}

		public static string EXCEPTION_BIND_FAILEDTOBINDGENERICARGUMENTSTOTYPE
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_BIND_FAILEDTOBINDGENERICARGUMENTSTOTYPE";
				}
				return "Failed to bind generic arguments '{0}' to type '{1}'.";
			}
		}

		public static string EXCEPTION_BIND_MISSINGMETHODPARAMETER
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_BIND_MISSINGMETHODPARAMETER";
				}
				return "Missing required method/indexer '{0}' parameter.";
			}
		}

		public static string EXCEPTION_BIND_UNABLETOBINDINDEXER
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_BIND_UNABLETOBINDINDEXER";
				}
				return "Unable to find indexing property on type '{0}' accepting specified arguments.";
			}
		}

		public static string EXCEPTION_PARSER_UNEXPECTEDTOKENWHILEOTHEREXPECTED
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_PARSER_UNEXPECTEDTOKENWHILEOTHEREXPECTED";
				}
				return "A one of these '{0}' tokens are expected.";
			}
		}

		public static string EXCEPTION_TOKENIZER_INVALIDCHARLITERAL
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_TOKENIZER_INVALIDCHARLITERAL";
				}
				return "Invalid char literal.";
			}
		}

		public static string EXCEPTION_TOKENIZER_UNEXPECTEDSYMBOL
		{
			get
			{
				int num = Resources.currentLanguageIdx;
				if (num != 0)
				{
					return "EXCEPTION_TOKENIZER_UNEXPECTEDSYMBOL";
				}
				return "Unexpected symbol '{0}'.";
			}
		}

		private static readonly string[] supportedLanguages = new string[] { "en" };

		[ThreadStatic]
		private static int currentLanguageIdx;
	}
}
