extern _printf
extern _scanf
global _main

 section .bss 

 section .data 
	_int: dd 0
	_r: dd 0.0
	string_1: db ' ',0
	format_2: db '%d%s%g',0
	real: dd  0.0

 section .text 
	_main :
	push dword 6
	pop dword [_int]
	sub esp, 4
	mov [real], dword __float32__(2.48)
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
	push string_1
	push dword [_int]
	push format_2
	call _printf
	add esp, 16
	mov eax, 0
	ret 
