alias hz='./hz.sh'

HZWORDS=$(./hz.sh help-commands)

_hz() {
    local cur
    # COMP_WORDS is an array containing all individual words in the current command line
    # COMP_CWORD is the index of the word contianing the current cursor position
    # COMPREPLY is an array variable from which bash reads the possible completions
    cur=${COMP_WORDS[COMP_CWORD]}
    COMPREPLY=()
    # compgen returns the array of elements from $HZWORDS matching the current word
    COMPREPLY=( $( compgen -W "$HZWORDS" -- $cur ) )
    return 0
}

complete -F _hz hz

