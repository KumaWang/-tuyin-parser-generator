grammar Markdown;

markdown        : EOL* block*;

block           : (paragraph | header) (EOL+ | EOF);

raw_stream      : (WORD BLANK*)+ ;
italics_stream  : ITALICS_ENVELOPE BLANK* w_stream ITALICS_ENVELOPE BLANK*;
bold_stream     : BOLD_ENVELOPE BLANK* w_stream BOLD_ENVELOPE BLANK*;

w_stream        : (italics_stream | bold_stream | WORD BLANK*)+ | WORD;

header          : HEADER_START+ BLANK* raw_stream;
paragraph       : w_stream ;

//fragment ALNUM          : [a-zA-Z0-9] ;
fragment ALNUM          : ~[_*#\n\r\f\t ] ; // 'alnum' here means any char not white space or reserved by markdown
HEADER_START            : '#' ;

BLANK                   : (' ' | '\t') -> skip ;
EOL                     : ('\n' | '\r\f');
WHITESPACE              : EOL | BLANK ;

WORD                    : (ALNUM)+ ;
BOLD_ENVELOPE           : '*' ;
ITALICS_ENVELOPE        : '_' ;