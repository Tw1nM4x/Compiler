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
	_ar_from2 : equ 0
	_ar_size2 : equ (5 - _ar_from2 + 1)
	_ar: resd _ar_size0 * _ar_size1 * _ar_size2

 section .data 
	_x: dd 0
	_y: dd 0
	_z: dd 0
	finalVal16: dd  0
	finalVal24: dd  0
	finalVal32: dd  0
	format_1: db '%d',0
	real: dd  0.0

 section .text 
	_main :
	push dword 0
	pop eax
	mov [_x], eax
	push dword 5
	pop eax
	mov [finalVal16], eax
	Do_16:
	push dword 5
	pop eax
	mov [_y], eax
	push dword 10
	pop eax
	mov [finalVal24], eax
	Do_24:
	push dword 0
	pop eax
	mov [_z], eax
	push dword 5
	pop eax
	mov [finalVal32], eax
	Do_32:
	push dword [_x]
	push dword [_y]
	pop eax
	pop ebx
	add eax, ebx
	push eax
	push dword [_z]
	pop eax
	pop ebx
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
	cmp eax, dword [finalVal32]
	jle Do_32
	inc dword [_y]
	mov eax, dword [_y]
	cmp eax, dword [finalVal24]
	jle Do_24
	inc dword [_x]
	mov eax, dword [_x]
	cmp eax, dword [finalVal16]
	jle Do_16
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
