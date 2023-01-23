extern _printf
extern _scanf
global _main
section .data
section .text

 section .bss 
	_ar_from0 : equ 0
	_ar_size0 : equ (5 - _ar_from0 + 1)
	_ar_from1 : equ 5
	_ar_size1 : equ (10 - _ar_from1 + 1)
	_ar: resd _ar_size0 * _ar_size1

 section .data 
	_i: dd 0
	_j: dd 0
	finalVal13: dd  0
	finalVal21: dd  0
	format_1: db '%d',0
	real: dd  0.0

 section .text 
	_main :
	push dword 0
	pop eax
	mov [_i], eax
	push dword 5
	pop eax
	mov [finalVal13], eax
	Do_13:
	push dword 5
	pop eax
	mov [_j], eax
	push dword 10
	pop eax
	mov [finalVal21], eax
	Do_21:
	push dword [_i]
	push dword [_j]
	pop eax
	pop ebx
	add eax, ebx
	push eax
	mov ecx, 0
	push dword [_i]
	pop eax
	mov ebx, _ar_from0
	sub eax, ebx
	mov ebx, _ar_size0
	imul ebx
	add ecx, eax
	push dword [_j]
	pop eax
	mov ebx, _ar_from1
	sub eax, ebx
	add ecx, eax
	pop dword [_ar + 4 * ecx]
	inc dword [_j]
	mov eax, dword [_j]
	cmp eax, dword [finalVal21]
	jle Do_21
	inc dword [_i]
	mov eax, dword [_i]
	cmp eax, dword [finalVal13]
	jle Do_13
	mov ecx, 0
	push dword 3
	pop eax
	mov ebx, _ar_from0
	sub eax, ebx
	mov ebx, _ar_size0
	imul ebx
	add ecx, eax
	push dword 7
	pop eax
	mov ebx, _ar_from1
	sub eax, ebx
	add ecx, eax
	push dword [_ar + 4 * ecx]
	push format_1
	call _printf
	add esp, 4
	mov eax, 0
	ret 
