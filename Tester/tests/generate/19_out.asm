extern _printf
extern _scanf
global _main

 section .bss 

 section .data 
	format_1: db '%d',0
	real: dd  0.0

 section .text 
	_proc:
	push ebp
	mov ebp, esp
	sub esp, 4
	push dword 5
	pop dword [ebp - 4]
	push dword [ebp + 8]
	push dword [ebp - 4]
	pop ebx
	pop eax
	add eax, ebx
	push eax
	push format_1
	call _printf
	add esp, 4
	mov eax, 0
	add esp, 4
	mov esp, ebp
	pop ebp
	ret 
	
	_main :
	push dword 6
	call _proc
	ret 
