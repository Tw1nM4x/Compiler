extern _printf
extern _scanf
global _main

 section .bss 
	_ar_from0 : equ 0
	_ar_size0 : equ (5 - _ar_from0 + 1)
	_ar_from1 : equ 5
	_ar_size1 : equ (10 - _ar_from1 + 1)
	_ar_from2 : equ 0
	_ar_size2 : equ (5 - _ar_from2 + 1)
	_ar: resd _ar_size0 * _ar_size1 * _ar_size2

 section .data 
	_x: dd 0
	_y: dd 0
	_z: dd 0
	finalVal14: dd  0
	finalVal22: dd  0
	finalVal30: dd  0
	format_1: db '%d',0
	real: dd  0.0

 section .text 
	_main :
	push dword 0
	pop eax
	mov [_x], eax
	push dword 5
	pop eax
	mov [finalVal14], eax
	Do_14:
	push dword 5
	pop eax
	mov [_y], eax
	push dword 10
	pop eax
	mov [finalVal22], eax
	Do_22:
	push dword 0
	pop eax
	mov [_z], eax
	push dword 5
	pop eax
	mov [finalVal30], eax
	Do_30:
	push dword [_x]
	push dword [_y]
	pop ebx
	pop eax
	add eax, ebx
	push eax
	push dword [_z]
	pop ebx
	pop eax
	sub eax, ebx
	push eax
	mov ecx, 0
	push dword [_x]
	pop eax
	mov ebx, _ar_from0
	sub eax, ebx
	mov ebx, _ar_size0
	imul ebx
	add ecx, eax
	push dword [_y]
	pop eax
	mov ebx, _ar_from1
	sub eax, ebx
	mov ebx, _ar_size1
	imul ebx
	add ecx, eax
	push dword [_z]
	pop eax
	mov ebx, _ar_from2
	sub eax, ebx
	add ecx, eax
	pop dword [_ar + 4 * ecx]
	inc dword [_z]
	mov eax, dword [_z]
	cmp eax, dword [finalVal30]
	jle Do_30
	inc dword [_y]
	mov eax, dword [_y]
	cmp eax, dword [finalVal22]
	jle Do_22
	inc dword [_x]
	mov eax, dword [_x]
	cmp eax, dword [finalVal14]
	jle Do_14
	mov ecx, 0
	push dword 3
	pop eax
	mov ebx, _ar_from0
	sub eax, ebx
	mov ebx, _ar_size0
	imul ebx
	add ecx, eax
	push dword 5
	pop eax
	mov ebx, _ar_from1
	sub eax, ebx
	mov ebx, _ar_size1
	imul ebx
	add ecx, eax
	push dword 1
	pop eax
	mov ebx, _ar_from2
	sub eax, ebx
	add ecx, eax
	push dword [_ar + 4 * ecx]
	push format_1
	call _printf
	add esp, 4
	mov eax, 0
	ret 
