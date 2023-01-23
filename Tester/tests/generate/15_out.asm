extern _printf
extern _scanf
global _main
section .data
section .text
_l equ 2

 section .bss 
	_arstr_from0 : equ 0
	_arstr_size0 : equ (((10 - 5) + _l) - _arstr_from0 + 1)
	_arstr: resd _arstr_size0
	_arint_from0 : equ 3
	_arint_size0 : equ (5 - _arint_from0 + 1)
	_arint: resd _arint_size0
	_arreal_from0 : equ (1 + 2)
	_arreal_size0 : equ (5 - _arreal_from0 + 1)
	_arreal: resd _arreal_size0

 section .data 
	string_1: db 'hello',0
	string_2: db ' ',0
	string_3: db ' ',0
	format_4: db '%s%s%d%s%g',0
	real: dd  0.0

 section .text 
	_main :
	push string_1
	mov ecx, 0
	push dword 5
	pop eax
	mov ebx, _arstr_from0
	sub eax, ebx
	add ecx, eax
	pop dword [_arstr + 4 * ecx]
	push dword 5
	mov ecx, 0
	push dword 4
	pop eax
	mov ebx, _arint_from0
	sub eax, ebx
	add ecx, eax
	pop dword [_arint + 4 * ecx]
	push dword 4
	mov ecx, 0
	push dword 4
	push dword 1
	pop eax
	pop ebx
	sub eax, ebx
	push eax
	pop eax
	mov ebx, _arint_from0
	sub eax, ebx
	add ecx, eax
	pop dword [_arint + 4 * ecx]
	sub esp, 4
	mov [real], dword __float32__(4.20)
	fld dword [real]
	fstp dword [esp]
	mov ecx, 0
	push dword 5
	pop eax
	mov ebx, _arreal_from0
	sub eax, ebx
	add ecx, eax
	pop dword [_arreal + 4 * ecx]
	mov ecx, 0
	push dword 5
	pop eax
	mov ebx, _arreal_from0
	sub eax, ebx
	add ecx, eax
	push dword [_arreal + 4 * ecx]
	mov ecx, 0
	push dword 4
	pop eax
	mov ebx, _arint_from0
	sub eax, ebx
	add ecx, eax
	push dword [_arint + 4 * ecx]
	pop eax
	mov [real], eax
	fild dword [real]
	sub esp, 4
	fstp dword [esp]
	fld dword [esp + 4]
	fld dword [esp]
	fadd 
	add esp, 4
	fstp dword [esp]
	pop eax
	mov [real], eax
	fld dword [real]
	sub esp, 8
	fstp qword [esp]
	push string_2
	mov ecx, 0
	push dword 4
	pop eax
	mov ebx, _arint_from0
	sub eax, ebx
	add ecx, eax
	push dword [_arint + 4 * ecx]
	mov ecx, 0
	push dword 3
	pop eax
	mov ebx, _arint_from0
	sub eax, ebx
	add ecx, eax
	push dword [_arint + 4 * ecx]
	pop eax
	pop ebx
	sub eax, ebx
	push eax
	push string_3
	mov ecx, 0
	push dword 5
	pop eax
	mov ebx, _arstr_from0
	sub eax, ebx
	add ecx, eax
	push dword [_arstr + 4 * ecx]
	push format_4
	call _printf
	add esp, 24
	mov eax, 0
	ret 
