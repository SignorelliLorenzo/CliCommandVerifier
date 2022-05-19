# MANUALE D’USO CLI CliCommandVerifier

## ISTRUZIONI PER L’INSERIMENTO DI COMANDI

I comandi sono da inserire all’interno del file “Commands.txt” uno per riga i comandi stessi hanno
la possibilità di avere dei parametri di 5 tipi:

* PARAM: parametro generico
* INT: intero
* MASK: maschera di rete
* IP: ip di rete
* COMBO.op1.op2 : valori stringa possibili nel comando
Per aggiungere un parametro lo bisogna inserire tra ‘\*’ (\*PARAM\*) e se ci sono più parametri essi devono essere
divisi dal simbolo ‘|’ (\*PARAM|PARAM\*) i comandi possono essere facoltativi ma essi devono essere messi alla fine e vanno preceduti da ‘?’ 
(\*PARAM|?PARAM\*).

## ISTRUZIONI PER L’OUTPUT

L’output contiene i comandi completi e evidenzierà gli errori nel seguente modo:

* “\*comando\*” il comando è errato o incompleto
* “\*?comando?\*” il comando precedente a questo è errato ma il comando di per se è corretto
* \*PARAM* se un parametro obbligatorio risulta mancante

NB. I comandi successivi a uno errato sono di per se errati




