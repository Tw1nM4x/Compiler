extern _printf
extern _scanf
global _main
section .data
section .text

 section .bss 

 section .data 
	string_1: db 'Hello, World!',0
	format_2: db '%s',0
	real: dd  0.0

 section .text 
	_main :
	push string_1
	push format_2
	call _printf
	add esp, 4
	mov eax, 0
	ret 
