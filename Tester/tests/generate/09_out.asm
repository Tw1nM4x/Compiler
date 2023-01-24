extern _printf
extern _scanf
global _main
_c1 equ 5
_c2 equ string_1
_c3 equ __float32__(5.30)

 section .bss 

 section .data 
	string_1: db 'const',0
	_int: dd 0
	string_2: db ' ',0
	string_3: db ' ',0
	format_4: db '%d%s%s%s%g',0
	real: dd  0.0

 section .text 
	_main :
	push dword 4
	pop dword [_int]
	push _c3
	pop eax
	mov [real], eax
	fld dword [real]
	sub esp, 8
	fstp qword [esp]
	push string_2
	push _c2
	push string_3
	push dword [_int]
	push format_4
	call _printf
	add esp, 24
	mov eax, 0
	ret 
