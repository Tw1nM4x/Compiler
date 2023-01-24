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
	push dword 5
	pop dword [_a]
	jmp Check_7
	Do_7:
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
	sub eax, ebx
	push eax
	pop dword [_a]
	Check_7:
	push dword [_a]
	push dword 0
	pop ebx
	pop eax
	cmp eax, ebx
	jg True_29
	push byte 0
	jmp False_29
	True_29:
	push byte 1
	False_29:
	pop eax
	cmp eax, byte 1
	je Do_7
	ret 
