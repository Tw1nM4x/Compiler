extern _printf
extern _scanf
global _main
section .data
section .text

struc rec_rec
._x: resd 1
._y: resd 1
._z: resd 1
endstruc 

 section .bss 

 section .data 
	
	_ex:
	istruc rec_rec
	at rec_rec._x, dd 0
	at rec_rec._y, dd 0
	at rec_rec._z, dd 0
	iend 
	
	string_1: db 'test',0
	string_2: db ' ',0
	string_3: db ' ',0
	format_4: db '%d%s%s%s%g',0
	real: dd  0.0

 section .text 
	_main :
	push dword 10
	push dword 6
	pop ebx
	pop eax
	mov ecx, 0
	mov edx, 0
	idiv ebx
	push eax
	pop dword [_ex + rec_rec._x]
	push string_1
	pop dword [_ex + rec_rec._y]
	sub esp, 4
	mov [real], dword __float32__(5.40)
	fld dword [real]
	fstp dword [esp]
	push dword [_ex + rec_rec._x]
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
	pop dword [_ex + rec_rec._z]
	push dword [_ex + rec_rec._z]
	pop eax
	mov [real], eax
	fld dword [real]
	sub esp, 8
	fstp qword [esp]
	push string_2
	push dword [_ex + rec_rec._y]
	push string_3
	push dword [_ex + rec_rec._x]
	push format_4
	call _printf
	add esp, 24
	mov eax, 0
	ret 
