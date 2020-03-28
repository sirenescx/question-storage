function addNewRow(tableId) {
    const tableRef = document.getElementById(tableId);
    const newRow = tableRef.insertRow(-1);
    const cell1 = newRow.insertCell(0);
    const cell2 = newRow.insertCell(1);
    if (tableId === "answerTable") {
        cell1.innerHTML = '<textarea name="AnswerText" class="textarea" rows="1" cols="60" width="100%"></textarea>';
        cell2.innerHTML = '<input type="hidden" value="off" name="Correct"><input name="Correct" type="checkbox" id="correct" value="on"><label for="correct">Correct</label>';
        const cell3 = newRow.insertCell(2);
        cell3.innerHTML = '<button class="remove-button" onclick="removeRow(this, \'answerTable\')">✕</button>';
    } else if (tableId === "tagTable") {
        cell1.innerHTML = '<textarea name="TagName" class="textarea" rows="1" cols="60" width="100%"></textarea>';
        cell2.innerHTML = '<input name="Use" type="checkbox" id="use"><label for="use">Use</label>';
    }
}

function removeRow(oButton, table) {
    document.getElementById(table).deleteRow(oButton.parentNode.parentNode.rowIndex);
}