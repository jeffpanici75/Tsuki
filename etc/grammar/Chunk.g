//
// Tsuki
//
// The MIT License (MIT)
// 
// Copyright (c) 2014 Jeff Panici
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

grammar Chunk;

options {
    language=CSharp3;
	TokenLabelType=CommonToken;
    output=AST;
    ASTLabelType=CommonTree;
    backtrack=true;
    k=2;
    memoize=true;
}

tokens {
    Chunk;
    Block;
    NamedKVField;
 	KVField;
	ArrayField;
	TableDef;
	Nil;
	False;
	True;
	VarArgsRef;
    Args;
    VarArgs;
    Return;
    UnaryMinusOp;
	Nested;
    Rooted;	
    Assign;
    Scope;
    If;
    ElseIf;
    Else;
    While;
    Repeat;
    Range;
    Iter;	
    RHS;
    LHS;
    LocalVar;
    Defun;
    AnonDefun;
    Standalone;
    NestedStandalone;
    IndexedSuffix;
    PropertySuffix;
    Prefix;
    Invoke;
    MethodInvoke;
    NestedPrefix;
    Args;
    Break;
    Continue;
    FName;
    ImpliedSelfFuncName;
    FuncName;
}

@parser::namespace { PaniciSoftware.Tsuki.Common }
@lexer::namespace { PaniciSoftware.Tsuki.Common }
@lexer::header { #pragma warning disable 168 }
@parser::header {  #pragma warning disable 168 }

@parser::members {
    private readonly ErrorList _errors = new ErrorList();

    public ErrorList Errors { get { return _errors; } }

    public override void DisplayRecognitionError(string[] tokenNames, RecognitionException e)
    {
        var header = this.GetErrorHeader(e);
        var message = this.GetErrorMessage(e, tokenNames);
        Errors.RecognitionError( header, message );
        this.EmitErrorMessage( header + " " + message );
    }
}

@lexer::members {
    private readonly ErrorList _errors = new ErrorList();

    public ErrorList Errors { get { return _errors; } }

    private void FoundUnsupportedEscape() {
        Errors.UnsupportedEscapeFound("Binary Literal");
    }

    public bool noCloseAhead(int numEqSigns) {
        if(input.LA(1) != ']') return true;
        for(int i = 2; i < numEqSigns+2; i++) {
            if(input.LA(i) != '=') return true;
        }
        return input.LA(numEqSigns+2) != ']';
    }

    public void matchClose(int numEqSigns) {
        var eqSigns = new System.Text.StringBuilder();
        for(int i = 0; i < numEqSigns; i++) {
            eqSigns.Append('=');
        }
        Match("]"+eqSigns+"]");
    }
}

public chunk: stat* EOF -> ^( Chunk stat* );

block: stat* -> ^( Block stat* );

SemiColon: ';';

stat
    : SemiColon
	| varlist '=' explist -> ^( Assign ^( LHS varlist ) ^( RHS explist ) )
    | 'function' funcname '(' paramlist? ')' block 'end' -> ^( Defun block funcname paramlist? )
    | 'local' 'function' Name '(' paramlist? ')' block 'end'
        -> ^( LocalVar ^( LHS Name ) ^( RHS ^( AnonDefun block paramlist? ) ))
    | 'local' namelist ('=' explist)? -> ^( LocalVar ^( LHS namelist ) ^( RHS explist )? )
	| do -> do
    | while -> while
    | repeat -> repeat
    | iter -> iter
    | range -> range
    | ifblock -> ifblock
    | functioncall -> functioncall
    | 'break' -> Break
    | 'continue' -> Continue
    | 'return' explist? -> ^( Return explist? )
	;

do: 'do' block 'end' -> ^( Scope block );
while:  'while' exp 'do' block 'end' -> ^( While block exp );
repeat: 'repeat' block 'until' exp -> ^( Repeat block exp );
range: 'for' n=Name '=' l=exp ',' h=exp (',' s=exp )? 'do' b=block 'end' -> ^( Range $b $n $l $h $s? );
iter: 'for' namelist 'in' explist 'do' block 'end' -> ^( Iter block ^( LHS namelist ) ^( RHS explist ) );
ifblock: 'if' iCond=exp 'then'
    	iBlock=block
        ( 'elseif' eiCond+=exp 'then'
            eiBlock+=block )*
        ( 'else' eBlock=block )?
        'end'	-> ^( If $iCond $iBlock ^( Else $eBlock )? ^( ElseIf $eiCond $eiBlock )* );

funcname : root=Name ('.' n+=Name)* 
        ( (':' n+=Name) -> ^( ImpliedSelfFuncName ^( Rooted $root ^( PropertySuffix $n )* ) )
        | -> ^( FuncName ^( Rooted $root ^( PropertySuffix $n )* ) ) );

functioncall
    : var nameAndArgs+ -> ^( Standalone var nameAndArgs+ )
    | '(' exp ')' nameAndArgs+ -> ^( NestedStandalone exp nameAndArgs+ )
    ;

var
    : Name varSuffix* -> ^( Rooted Name varSuffix* )
    | '(' exp ')' varSuffix+ -> ^( Nested exp varSuffix+ );

varSuffix: nameAndArgs*
        ('[' exp ']' -> ^( IndexedSuffix exp nameAndArgs* )
        | '.' Name  -> ^( PropertySuffix Name nameAndArgs* ) );

prefixexp
    : var nameAndArgs* -> ^( Prefix var nameAndArgs* )
    | '(' exp ')' nameAndArgs* -> ^( NestedPrefix exp nameAndArgs* )
    ;

nameAndArgs
    : ':' Name args -> ^( MethodInvoke Name args )
    | args -> ^( Invoke args )
    ;

args
    : '(' explist? ')' -> ^( Args explist? )
    | tableconstructor -> ^( Args tableconstructor )
    | string -> ^( Args string )
    ;

varlist: var (',' var )* -> var+;

namelist: Name (',' Name )* -> Name+;

explist: exp (',' exp )* -> exp+;

paramlist
    : namelist (',' '...' -> ^( VarArgs namelist ) | -> ^( Args namelist ) )
    | '...' -> ^( VarArgs );

OrOp: 'or';

exp: andexp ( OrOp^ andexp )*;

AndOp: 'and';

andexp: eqexp ( AndOp^ eqexp )*;

EQ: '==';
NEQ: '~=';

eqexp: relationalexp ( (EQ|NEQ)^ relationalexp )*;

LT: '<';
GT: '>';
LE: '<=';
GE: '>=';

relationalexp: concatenateexp ( (LT|GT|LE|GE)^ concatenateexp )*;

ConcatenateOp: '..';

concatenateexp: (addexp ConcatenateOp^)* addexp;

MinusOp: '-';
AddOp: '+';

addexp: mulexp ( ( MinusOp | AddOp )^ mulexp )*;

MultiplyOp: '*';
DivOp: '/';
ModOp: '%';

mulexp: unaryexp ( ( MultiplyOp | DivOp | ModOp )^ unaryexp )*;

NotOp: 'not';
LengthOp: '#';

unaryexp
    : NotOp unaryexp -> ^( NotOp unaryexp )
    | LengthOp unaryexp -> ^( LengthOp unaryexp )
    | MinusOp unaryexp -> ^( UnaryMinusOp unaryexp )
    | powerexp -> powerexp
    ;

PowerOp: '^';
powerexp: ( primaryexp PowerOp^)* primaryexp;

primaryexp
    : 'nil' -> Nil
    | 'false' -> False
    | 'true' -> True
    | prefixexp -> prefixexp
    | tableconstructor -> tableconstructor
    | string -> string
    | number -> number
    | anondefun -> anondefun
    | '...' -> ^(Prefix ^(Rooted '...' ) )
    ;

anondefun: 'function' '(' paramlist? ')' block 'end' -> ^( AnonDefun block paramlist? );

tableconstructor : '{' (fieldlist)? '}' -> ^( TableDef fieldlist? );

fieldlist : f+=field (fieldsep f+=field)* (fieldsep)? -> $f+;

field
    : '[' k=exp ']' '=' v=exp -> ^( KVField $k $v )
    | Name '=' exp -> ^( NamedKVField Name exp )
    | exp -> ^( ArrayField exp )
    ;

fieldsep : ',' | ';';

number
    : Int
    | Float
    | Exponent
    | Hex
    | HexFloat
    | HexExponent
    ;

string
    : String
    | CharString
    | LongBracket
    ;

Name:('a'..'z'|'A'..'Z'|'_')(options{greedy=true;}:	'a'..'z'|'A'..'Z'|'_'|'0'..'9')*;

Int: ('0'..'9')+;

Float: Int '.' Int ;

Exponent: (Int| Float) ('E'|'e') ('-'|'+')? Int;

HexExponent: (Hex| HexFloat) ('P'|'p') ('-'|'+')? HexDigit+;

HexFloat: ( '0x' | '0X' ) HexDigit+ '.' HexDigit+;

Hex: ( '0x' | '0X' ) HexDigit+;

String
@init{ var buf = new System.Text.StringBuilder(); }
    :  '"' 
        ( 
        EscapeSequence { buf.Append( Text ); }
        | ch=~('\\'|'"') { buf.Append((char)$ch); } 
        | '\\"' { buf.Append('"'); }
        )* 
        '"'
        { Text = buf.ToString(); }
    ;

CharString
@init{ var buf = new System.Text.StringBuilder(); }
    :  '\'' 
        ( 
        EscapeSequence { buf.Append( Text ); }
        | ch=~('\\'|'\'') { buf.Append((char)$ch); } 
        | '\\\'' { buf.Append('\''); } 
        )* 
        '\''
        { Text = buf.ToString(); }
    ;

fragment
EscapeSequence 
    : '\\b' { Text = "\b"; }
    | '\\t' { Text = "\t"; }
    | '\\n' { Text = "\n"; }
    | '\\f' { Text = "\f"; }
    | '\\r' { Text = "\r"; }
    | '\\\\' { Text = "\\"; }
    | UnicodeEscape
    | UnsupportedEsacpe
    ;

fragment
UnsupportedEsacpe
    : '\\' ('0'..'9') ( ('0'..'9') ('0'..'9')? )? { Text = ""; FoundUnsupportedEscape(); };

fragment
UnicodeEscape: '\\' 'u' one=HexDigit two=HexDigit three=HexDigit four=HexDigit
        {   var bldr = new System.Text.StringBuilder();
            bldr.Append( $one.Text );
            bldr.Append( $two.Text );
            bldr.Append( $three.Text );
            bldr.Append( $four.Text );
            var codepoint = int.Parse( bldr.ToString(), System.Globalization.NumberStyles.HexNumber );
            Text = char.ConvertFromUtf32( codepoint ); };

fragment HexDigit: ( '0' .. '9' | 'a' .. 'f' | 'A' .. 'F' );

NewLine: ('\r')? '\n' {Skip();};

WS: (' '|'\t'|'\u000C') {Skip();};

Comment
  : ( BlockComment
  | LineComment ) {Skip();}
  ;

fragment
BlockComment
  :  '--' LongBracket
  ;

fragment
LineComment
  :  '--' (~('[' | '\r' | '\n') ~('\r' | '\n')*)? ('\r'? '\n' | EOF)
  ;

LongBracket
@init {int openEq = 0; var bldr = new System.Text.StringBuilder();}
    :  '[' ('=' {openEq++;})* '[' ({noCloseAhead(openEq)}?=> ch=.{bldr.Append((char)$ch);})*
        {
            matchClose(openEq); 
            Text = bldr.ToString();
        }
    ;
