extern _printf
extern _scanf
global _main
section .data
section .text

 section .bss 

 section .data 
	_int: dd 0
	_r: dd 0.0
	string_1: db ' ',0
	format_2: db '%d%s%g',0
	real: dd  0.0

 section .text 
	_main :
	push dword 2
	push dword 2
	push dword 4
	push dword 2
	pop ebx
	pop eax
	sub eax, ebx
	push eax
	pop ebx
	pop eax
	imul ebx
	push eax
	pop ebx
	pop eax
	add eax, ebx
	push eax
	pop dword [_int]
	sub esp, 4
	mov [real], dword __float32__(6.20)
	fld dword [real]
	fstp dword [esp]
	sub esp, 4
	mov [real], dword __float32__(2.50)
	fld dword [real]
	fstp dword [esp]
	fld dword [esp + 4]
	fld dword [esp]
	fdiv 
	add esp, 4
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
