function removeRow(oButton, values) {
    document.getElementById(values).deleteRow(oButton.parentNode.parentNode.rowIndex);
}