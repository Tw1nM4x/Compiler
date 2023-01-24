extern _printf
extern _scanf
global _main

 section .bss 

 section .data 
	_a: dd 0
	string_1: db 'false',0
	format_2: db '%s',0
	string_3: db 'true',0
	format_4: db '%s',0
	real: dd  0.0

 section .text 
	_main :
	push dword 5
	pop dword [_a]
	push dword [_a]
	push dword 4
	pop ebx
	pop eax
	cmp eax, ebx
	je True_11
	push byte 0
	jmp False_11
	True_11:
	push byte 1
	False_11:
	pop eax
	cmp eax, byte 1
	je Then_20
	push string_1
	push format_2
	call _printf
	add esp, 4
	mov eax, 0
	jmp Else_20
	Then_20:
	push string_3
	push format_4
	call _printf
	add esp, 4
	mov eax, 0
	Else_20:
	ret 
