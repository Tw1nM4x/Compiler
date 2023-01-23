extern _printf
extern _scanf
global _main
section .data
section .text

struc rec_rec1
._x: resd 1
endstruc 

struc rec_rec2
._x: resd 1
._y: resd 1
endstruc 

 section .bss 

 section .data 
	
	_ex1:
	istruc rec_rec1
	at rec_rec1._x, dd 0
	iend 
	
	
	_ex2:
	istruc rec_rec2
	at rec_rec2._x, dd 0
	at rec_rec2._y, dd 0
	iend 
	
	string_1: db ' ',0
	string_2: db ' ',0
	format_3: db '%d%s%d%s%d',0
	real: dd  0.0

 section .text 
	_main :
	push dword 5
	pop dword [_ex1 + rec_rec1._x]
	push dword 7
	pop dword [_ex2 + rec_rec2._x]
	push dword [_ex1 + rec_rec1._x]
	push dword [_ex2 + rec_rec2._x]
	pop ebx
	pop eax
	add eax, ebx
	push eax
	pop dword [_ex2 + rec_rec2._y]
	push dword [_ex2 + rec_rec2._y]
	push dword 1
	pop ebx
	pop eax
	sub eax, ebx
	push eax
	pop dword [_ex1 + rec_rec1._x]
	push dword [_ex2 + rec_rec2._y]
	push string_1
	push dword [_ex2 + rec_rec2._x]
	push string_2
	push dword [_ex1 + rec_rec1._x]
	push format_3
	call _printf
	add esp, 20
	mov eax, 0
	ret 
