extern _printf
extern _scanf
global _main
section .data
section .text

 section .bss 

 section .data 
	_int: dd 0
	format_1: db '%d',0
	real: dd  0.0

 section .text 
	_main :
	push dword 10
	pop dword [_int]
	push dword [_int]
	push format_1
	call _printf
	add esp, 4
	mov eax, 0
	ret 
