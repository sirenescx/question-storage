function checkInput(ob) {
    let invalidChars = /[^0-9]/gi;
    if (invalidChars.test(ob.value)) {
        ob.value = ob.value.replace(invalidChars,"");
    }
}