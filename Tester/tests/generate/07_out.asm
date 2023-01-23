extern _printf
extern _scanf
global _main
section .data
section .text

 section .bss 

 section .data 
	_int: dd 0
	_r: dd 0.0
	format_1: db '%g',0
	real: dd  0.0

 section .text 
	_main :
	push dword 5
	pop dword [_int]
	sub esp, 4
	mov [real], dword __float32__(5.20)
	fld dword [real]
	fstp dword [esp]
	pop dword [_r]
	sub esp, 4
	fld dword [_r]
	fstp dword [esp]
	push dword [_int]
	pop eax
	mov [real], eax
	fild dword [real]
	sub esp, 4
	fstp dword [esp]
	fld dword [esp + 4]
	fld dword [esp]
	fsub 
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
	push format_1
	call _printf
	add esp, 8
	mov eax, 0
	ret 
