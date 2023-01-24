extern _printf
extern _scanf
global _main

 section .bss 

 section .data 
	_a: dd 0
	finalVal5: dd  0
	string_1: db ' ',0
	format_2: db '%s%d',0
	real: dd  0.0

 section .text 
	_main :
	push dword 1
	pop eax
	mov [_a], eax
	push dword 5
	pop eax
	mov [finalVal5], eax
	Do_5:
	push dword [_a]
	push string_1
	push format_2
	call _printf
	add esp, 8
	mov eax, 0
	inc dword [_a]
	mov eax, dword [_a]
	cmp eax, dword [finalVal5]
	jle Do_5
	ret 
