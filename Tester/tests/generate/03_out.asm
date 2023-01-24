extern _printf
extern _scanf
global _main

 section .bss 

 section .data 
	_str: dd 0
	_int: dd 0
	_r: dd 0.0
	string_1: db 'Hello',0
	string_2: db ' ',0
	string_3: db ' ',0
	format_4: db '%s%s%d%s%g',0
	real: dd  0.0

 section .text 
	_main :
	push string_1
	pop dword [_str]
	push dword 100
	pop dword [_int]
	sub esp, 4
	mov [real], dword __float32__(4.25)
	fld dword [real]
	fstp dword [esp]
	pop dword [_r]
	sub esp, 4
	fld dword [_r]
	fstp dword [esp]
	pop eax
	mov [real], eax
	fld dword [real]
	sub esp, 8
	fstp qword [esp]
	push string_2
	push dword [_int]
	push string_3
	push dword [_str]
	push format_4
	call _printf
	add esp, 24
	mov eax, 0
	ret 
