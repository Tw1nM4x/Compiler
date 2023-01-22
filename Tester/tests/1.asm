extern _printf
extern _scanf
global _main
section .data
section .text
_i equ 5

 section .bss 
	_ar_from0 : equ 0
	_ar_size0 : equ (5 - _ar_from0 + 1)
	_ar: resd _ar_size0
	_ar2_from0 : equ 0
	_ar2_size0 : equ (5 - _ar2_from0 + 1)
	_ar2: resd _ar2_size0
	_ar3_from0 : equ 0
	_ar3_size0 : equ (5 - _ar3_from0 + 1)
	_ar3: resd _ar3_size0

 section .data 
	_a: dd 0.0
	_b: dd 0.0
	_c: dd 0
	format_1: db '%g',0
	string_2: db ' ',0
	format_3: db '%s%g',0
	string_4: db 'po',0
	string_5: db 'heu',0
	newstr_142 : times 10 db 0
	string_6: db 'hey',0
	format_7: db '%s%s',0
	real: dd  0.0

 section .text 
	_main :
	push dword 5
	pop eax
	mov [real], eax
	fild dword [real]
	sub esp, 4
	fstp dword [esp]
	pop dword [_a]
	sub esp, 4
	mov [real], dword __float32__(9.20)
	fld dword [real]
	fstp dword [esp]
	sub esp, 4
	mov [real], dword __float32__(3.10)
	fld dword [real]
	fstp dword [esp]
	fld dword [esp + 4]
	fld dword [esp]
	fadd 
	add esp, 4
	fstp dword [esp]
	pop dword [_b]
	sub esp, 4
	mov [real], dword __float32__(0.05)
	fld dword [real]
	fstp dword [esp]
	push dword 20
	pop eax
	mov [real], eax
	fild dword [real]
	sub esp, 4
	fstp dword [esp]
	fld dword [esp + 4]
	fld dword [esp]
	fmul 
	add esp, 4
	fstp dword [esp]
	pop dword [_a]
	sub esp, 4
	fld dword [_a]
	fstp dword [esp]
	sub esp, 4
	fld dword [_b]
	fstp dword [esp]
	fld dword [esp + 4]
	fld dword [esp]
	fsub 
	add esp, 4
	fstp dword [esp]
	pop dword [_a]
	sub esp, 4
	fld dword [_a]
	fstp dword [esp]
	pop eax
	mov [real], eax
	fld dword [real]
	sub esp, 8
	fstp qword [esp]
	push format_1
	call _printf
	add esp, 4
	mov eax, 0
	push dword 9
	mov ecx, 0
	push dword 2
	pop eax
	mov ebx, _ar2_from0
	sub eax, ebx
	add ecx, eax
	pop dword [_ar2 + 4 * ecx]
	sub esp, 4
	mov [real], dword __float32__(6.80)
	fld dword [real]
	fstp dword [esp]
	mov ecx, 0
	push dword 4
	pop eax
	mov ebx, _ar_from0
	sub eax, ebx
	add ecx, eax
	pop dword [_ar + 4 * ecx]
	mov ecx, 0
	push dword 4
	pop eax
	mov ebx, _ar_from0
	sub eax, ebx
	add ecx, eax
	push dword [_ar + 4 * ecx]
	mov ecx, 0
	push dword 2
	pop eax
	mov ebx, _ar2_from0
	sub eax, ebx
	add ecx, eax
	push dword [_ar2 + 4 * ecx]
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
	push format_3
	call _printf
	add esp, 4
	mov eax, 0
	push string_4
	push string_5
	pop ebx
	pop eax
	push dword [eax]
	pop dword [newstr_142]
	push dword [ebx]
	pop dword [newstr_142 + 4]
	push newstr_142
	pop dword [_c]
	push string_6
	mov ecx, 0
	push dword 4
	pop eax
	mov ebx, _ar3_from0
	sub eax, ebx
	add ecx, eax
	pop dword [_ar3 + 4 * ecx]
	push dword [_c]
	mov ecx, 0
	push dword 4
	pop eax
	mov ebx, _ar3_from0
	sub eax, ebx
	add ecx, eax
	push dword [_ar3 + 4 * ecx]
	push format_7
	call _printf
	add esp, 4
	mov eax, 0
	ret 
