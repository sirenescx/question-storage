function displayAnswerVariants() {
    const selected = document.getElementById("typeOfQuestionSelector").value;
    if (document.getElementById("answerTable") == null) {
        if (selected === "sc" || selected === "mc") {
            document.getElementById("answerInfo").innerHTML =
                '<p id="answerInfo"></p>' +
                '<label id="answerTableLabel" for="answerTable">Response options</label>' +
                '<table id="answerTable">' +
                    '<tr>' +
                        '<th></th>' +
                        '<th></th>' +
                        '<th></th><' +
                    '/tr>' +
                    '<tr>' +
                        '<td>' +
                            '<textarea class="textarea" name="AnswerText" rows="1" cols="60"></textarea>' +
                        '</td>' +
                        '<td>' +
                            '<input type="hidden" value="off" name="Correct">' +
                            '<input type="checkbox" name="Correct" id="correct" value="on">' +
                            '<label for="correct">Correct</label>' +
                        '</td>' +
                        '<td>' +
                            '<button class="remove-button" onclick="removeRow(this, \'answerTable\')">✕</button>' +
                        '</td>' +
                    '</tr>' +
                '</table>' +
                '<p><button class="button" id="addResponseOptions" type="button" onclick="addNewRow(\'answerTable\')"> Add new response option </button></p>';
        } else if (selected === "oa") {
            document.getElementById("answerInfo").innerHTML =
                '<label for="openAnswerText">Correct answer</label>' +
                '<p><textarea class="textarea" id="openAnswerText" rows="2" cols="100"></textarea></p>';
        }
    } else {
        if (selected === "sc" || selected === "mc") {
            document.getElementById("answerTable").innerHTML =
                '<table id="answerTable">' +
                    '<tr>' +
                        '<th></th>' +
                        '<th></th>' +
                        '<th></th>' +
                    '</tr>' +
                    '<tr>' +
                        '<td>' +
                            '<textarea name="AnswerText" class="textarea" rows="1" cols="60"></textarea>' +
                        '</td>' +
                        '<td>' +
                            '<input type="hidden" value="off" name="Correct">' +
                            '<input name="Correct" type="checkbox" id="correct" value="on">' +
                            '<label for="correct">Correct</label>' +
                        '</td>' +
                        '<td>' +
                            '<button class="remove-button" onclick="removeRow(this, \'answerTable\')">✕</button>' +
                        '</td>' +
                    '</tr>' +
                    '</table>';
        } else if (selected === "oa") {
            removeElement("answerTable");
            removeElement("answerTableLabel");
            removeElement("addAnswers");
            document.getElementById("answerInfo").innerHTML =
                '<label for="openAnswerText">Correct answer</label>' +
                '<p><textarea class="textarea" id="openAnswerText" rows="2" cols="100"></textarea></p>';
        }
    }
}

function removeElement(elementId) {
    let element = document.getElementById(elementId);
    element.parentNode.removeChild(element);
}