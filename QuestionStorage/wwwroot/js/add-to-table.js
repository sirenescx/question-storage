function addNewRow(tableId) {
    const tableRef = document.getElementById(tableId);
    const newRow = tableRef.insertRow(-1);
    let cell1 = newRow.insertCell(0);
    let cell2 = newRow.insertCell(1);
    let cell3 = newRow.insertCell(2);
    if (tableId === "answerTable") {
        let textarea = document.getElementsByName("AnswerOption.Answer")[0].cloneNode(false);
        textarea.value = "";
        cell1.appendChild(textarea);
        setCellStyle(cell2);
        setCellStyle(cell3);
        let selected = document.getElementById("typeOfQuestionSelector").value;
        if (selected === "sc") {
            cell2.innerHTML = '<input type="hidden" value="off" name="Correct">' +
                              '<input name="Correct" type="radio" value="on">';
        } else if (selected === "mc") {
            cell2.innerHTML = '<input type="hidden" value="off" name="Correct">' +
                              '<input name="Correct" type="checkbox" value="on">';
        }
        cell3.innerHTML = '<button id="addResponseOptions" onclick="removeRow(this, \'answerTable\')">✕</button>';
    } 
}

function removeRow(oButton, table) {
    document.getElementById(table).deleteRow(oButton.parentNode.parentNode.rowIndex);
}

function setCellStyle(cell) {
    cell.style.textAlign = "center";
    cell.style.verticalAlign = "top";
}


