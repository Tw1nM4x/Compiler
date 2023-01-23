extern _printf
extern _scanf
global _main
section .data
section .text

 section .bss 

 section .data 
	string_1: db ' ',0
	string_2: db ' ',0
	format_3: db '%d%s%s%s%g',0
	string_4: db 'test',0
	real: dd  0.0

 section .text 
	_proc:
	push ebp
	mov ebp, esp
	sub esp, 8
	push dword 5
	pop dword [ebp - 4]
	sub esp, 4
	mov [real], dword __float32__(0.50)
	fld dword [real]
	fstp dword [esp]
	pop dword [ebp - 8]
	push dword [ebp + 20]
	fld dword [esp + 4]
	fld dword [esp]
	fmul 
	add esp, 4
	fstp dword [esp]
	pop eax
	mov [real], eax
	fld dword [real]
	sub esp, 8
	fstp qword [esp]
	push string_1
	push dword [ebp + 16]
	push string_2
	push dword [ebp + 8]
	push dword [ebp + 12]
	pop ebx
	pop eax
	add eax, ebx
	push eax
	push dword [ebp - 4]
	pop ebx
	pop eax
	sub eax, ebx
	push eax
	push format_3
	call _printf
	add esp, 24
	mov eax, 0
	add esp, 16
	mov esp, ebp
	pop ebp
	ret 
	
	_main :
	sub esp, 4
	mov [real], dword __float32__(8.04)
	fld dword [real]
	fstp dword [esp]
	push string_4
	push dword 12
	push dword 8
	call _proc
	ret 
