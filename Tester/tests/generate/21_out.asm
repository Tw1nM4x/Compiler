extern _printf
extern _scanf
global _main
section .data
section .text

 section .bss 

 section .data 
	string_1: db ' ',0
	format_2: db '%d%s',0
	string_3: db ' ',0
	format_4: db '%d%s',0
	string_5: db ' ',0
	format_6: db '%d%s',0
	real: dd  0.0

 section .text 
	_proc1:
	push ebp
	mov ebp, esp
	sub esp, 0
	push string_1
	push dword 1
	push format_2
	call _printf
	add esp, 8
	mov eax, 0
	add esp, 0
	mov esp, ebp
	pop ebp
	ret 
	
	_proc2:
	push ebp
	mov ebp, esp
	sub esp, 0
	push string_3
	push dword 2
	push format_4
	call _printf
	add esp, 8
	mov eax, 0
	add esp, 0
	mov esp, ebp
	pop ebp
	ret 
	
	_proc3:
	push ebp
	mov ebp, esp
	sub esp, 0
	push string_5
	push dword 3
	push format_6
	call _printf
	add esp, 8
	mov eax, 0
	add esp, 0
	mov esp, ebp
	pop ebp
	ret 
	
	_main :
	call _proc1
	call _proc3
	call _proc2
	ret 
