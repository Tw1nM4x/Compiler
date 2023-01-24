extern _printf
extern _scanf
global _main

 section .bss 

 section .data 
	_str: dd 0
	string_1: db 'FizzBuzz',0
	format_2: db '%s',0
	real: dd  0.0

 section .text 
	_main :
	push string_1
	pop dword [_str]
	push dword [_str]
	push format_2
	call _printf
	add esp, 4
	mov eax, 0
	ret 
