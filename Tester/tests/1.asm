extern _printf
extern _scanf
global _main
section .data
var_l: dd 0
section .text
_main: 
push dword 6
pop eax
mov [var_l], eax
section .data
var_i: dd 0
section .text
push dword [var_l]
pop eax
mov [var_i], eax
section .text
push dword [var_i]
push dword 2
push dword 2
pop ebx
pop eax
add eax, ebx
push eax
pop ebx
pop eax
cmp eax, ebx
je True_27
push byte 0
jmp False_27
True_27:
push byte 1
False_27:
pop eax
cmp eax, byte 1
je Then_35
mov [format + 0], byte 'f'
mov [format + 1], byte 'a'
mov [format + 2], byte 'l'
mov [format + 3], byte 's'
mov [format + 4], byte 'e'
mov [format + 5], byte ' '
mov [format + 6], byte 0
push format
call _printf
add esp, 4
mov eax, 0
jmp Else_35
Then_35:
mov [format + 0], byte 't'
mov [format + 1], byte 'r'
mov [format + 2], byte 'u'
mov [format + 3], byte 'e'
mov [format + 4], byte ' '
mov [format + 5], byte 0
push format
call _printf
add esp, 4
mov eax, 0
Else_35:
push dword [var_l]
mov [format + 0], byte '%'
mov [format + 1], byte 'd'
mov [format + 2], byte 0
push format
call _printf
add esp, 4
mov eax, 0
section .data
real: dd  0.0
format: db '%s',0
