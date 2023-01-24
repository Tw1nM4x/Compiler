extern _printf
extern _scanf
global _main

 section .bss 

 section .data 
	_a: dd 0
	string_1: db ' ',0
	format_2: db '%s%d',0
	real: dd  0.0

 section .text 
	_main :
	push dword 2
	pop eax
	neg eax
	push eax
	pop dword [_a]
	Do_10:
	push dword [_a]
	push string_1
	push format_2
	call _printf
	add esp, 8
	mov eax, 0
	push dword [_a]
	push dword 1
	pop ebx
	pop eax
	add eax, ebx
	push eax
	pop dword [_a]
	push dword [_a]
	push dword 7
	pop ebx
	pop eax
	cmp eax, ebx
	jl True_30
	push byte 0
	jmp False_30
	True_30:
	push byte 1
	False_30:
	pop eax
	cmp eax, byte 1
	je Do_10
	ret 
