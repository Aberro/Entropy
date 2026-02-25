grammar MIPS;
options { language = CSharp; caseInsensitive = true; }

// Parser rules
program: line (NEWLINE line)*;
line:WS?label?WS?(instruction)?WS?comment?;
label:name COLON;
define:DEFINE WS name WS define_target;
alias:ALIAS WS name WS alias_target;
name:IDENTIFIER;
define_target:hash|NUMBER|HEX;
alias_target:register|device|IDENTIFIER;
instruction:define|alias|operation(WS operand(WS operand(WS operand(WS operand(WS operand(WS operand)?)?)?)?)?)?;
operation:IDENTIFIER;
operand:register|device|hash|NUMBER|HEX|IDENTIFIER;
comment:COMMENT;
register:REGISTER;
device:DEVICE;
hash:'HASH("'IDENTIFIER'")';

// Lexer rules
COMMENT:'#'~[\r\n]*;
ALIAS:'alias';
DEFINE:'define';
REGISTER:('r'+('0'|'1'[0-6]?|[1-9]|'a')|'sp');
DEVICE:('d''r'*('0'|'1'[0-6]?|[1-9])|'db')(':'('0'|'1'[0-6]?|[1-9]))?;
IDENTIFIER:([a-z]|'_')[a-z0-9_.]*;
NUMBER:'-'?[0-9]+('.'[0-9]+)?([e][+\-]?[0-9]+)?;
HEX:'$'[0-9a-f]+;
COMMA:',';
COLON:':';
NEWLINE:'\r'?'\n';
WS:[ \t]+;