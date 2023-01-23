extern _printf
extern _scanf
global _main
section .data
section .text

 section .bss 

 section .data 
	format_1: db '%d',0
	real: dd  0.0

 section .text 
	_main :
	push dword 5
	push dword 10
	mov eax, dword 2
	pop ebx
	imul ebx
	pop ebx
	add eax, ebx
	push eax
	push format_1
	call _printf
	add esp, 4
	mov eax, 0
	ret 
