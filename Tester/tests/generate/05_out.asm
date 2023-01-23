extern _printf
extern _scanf
global _main
section .data
section .text

 section .bss 

 section .data 
	_str: dd 0
	string_1: db 'Fizz',0
	string_2: db 'Buzz',0
	newstr_13 : times 9 db 0
	format_3: db '%s',0
	real: dd  0.0

 section .text 
	_main :
	push string_1
	push string_2
	pop ebx
	pop eax
	push dword [eax]
	pop dword [newstr_13]
	push dword [ebx]
	pop dword [newstr_13 + 4]
	push newstr_13
	pop dword [_str]
	push dword [_str]
	push format_3
	call _printf
	add esp, 4
	mov eax, 0
	ret 
