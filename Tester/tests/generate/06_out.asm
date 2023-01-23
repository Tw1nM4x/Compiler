extern _printf
extern _scanf
global _main
section .data
section .text

 section .bss 

 section .data 
	_int1: dd 0
	_int2: dd 0
	_int3: dd 0
	format_1: db '%d',0
	real: dd  0.0

 section .text 
	_main :
	push dword 5
	pop dword [_int1]
	push dword 2
	pop dword [_int2]
	push dword 9
	pop dword [_int3]
	push dword [_int3]
	push dword [_int1]
	pop ebx
	pop eax
	sub eax, ebx
	push eax
	push dword [_int2]
	pop ebx
	pop eax
	add eax, ebx
	push eax
	pop dword [_int3]
	push dword [_int3]
	push format_1
	call _printf
	add esp, 4
	mov eax, 0
	ret 
