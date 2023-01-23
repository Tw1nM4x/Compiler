extern _printf
extern _scanf
global _main
section .data
section .text

 section .bss 

 section .data 
	_a: dd 0
	finalVal7: dd  0
	string_1: db ' ',0
	format_2: db '%s%d',0
	real: dd  0.0

 section .text 
	_main :
	push dword 1
	pop eax
	mov [_a], eax
	push dword 5
	pop eax
	mov [finalVal7], eax
	Do_7:
	push dword [_a]
	push string_1
	push format_2
	call _printf
	add esp, 8
	mov eax, 0
	inc dword [_a]
	mov eax, dword [_a]
	cmp eax, dword [finalVal7]
	jle Do_7
	ret 
