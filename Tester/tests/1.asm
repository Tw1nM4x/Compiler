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
push dword 1
pop eax
mov [var_i], eax
push dword 9
pop eax
mov [finalVal], eax
Do_23:
push dword [var_i]
mov [format + 0], byte 'h'
mov [format + 1], byte 'e'
mov [format + 2], byte 'l'
mov [format + 3], byte 'l'
mov [format + 4], byte 'o'
mov [format + 5], byte ' '
mov [format + 6], byte '%'
mov [format + 7], byte 'd'
mov [format + 8], byte ' '
mov [format + 9], byte 0
push format
call _printf
add esp, 4
mov eax, 0
inc dword [var_i]
mov eax, dword [var_i]
cmp eax, dword [finalVal]
jle Do_23
section .data
finalVal: dd  0.0
real: dd  0.0
format: db '%s',0
