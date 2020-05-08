﻿const checkboxCode = '<input type="hidden" value="off" name="Correct">' +
                     '<input type="checkbox" value="on" name="Correct">';
const radioCode = '<input type="hidden" value="off" name="Correct">'+
                  '<input type="radio" name="Correct" value="on">';

function displayAnswerVariants() {
    const selected = document.getElementById("typeOfQuestionSelector").value;
    if (document.getElementById("answerTable") == null) {
        if (selected === "sc") {
            generateTableCode(radioCode, document.getElementById("answerInfo"), true);
        } else if (selected === "mc") {
            generateTableCode(checkboxCode, document.getElementById("answerInfo"), true);
        } else if (selected === "oa") {
            generateCorrectAnswerCode();
        }
    } else {
        if (selected === "sc") {
            generateTableCode(radioCode, document.getElementById("answerTable"), false);
        } else if (selected === "mc") {
            generateTableCode(checkboxCode, document.getElementById("answerTable"), false);
        } else if (selected === "oa") {
            removeElement("answerTable");
            removeElement("addResponseOptions");
            generateCorrectAnswerCode();
        }
    }
}

function removeElement(elementId) {
    let element = document.getElementById(elementId);
    element.parentNode.removeChild(element);
}

function generateTableCode(choiceCode, element, isNull) {
    if (document.getElementById("correctAnswerLabel") != null) {
        removeElement("correctAnswerLabel");
    }
    if (document.getElementById("correctAnswer") != null) {
        removeElement("correctAnswer");
    }
    
    element.innerHTML =                 
        '<table id="answerTable">' +
            '<tr>' +
                '<th style="font-weight: normal">Response options*</th>' +
                '<th style="font-weight: normal">Correct</th>' +
                '<th></th>' +
            '</tr>' +
            '<tr>' +
                '<td>' +
                    getTextAreaCode(1, '') +
                '</td>' +
                '<td style="text-align: center; vertical-align: top">' +
                    choiceCode +
                '</td>' +
                '<td>' +
                '</td>' +
            '</tr>' +
            '<tr>' +
                '<td style="text-align: center; vertical-align: top">' +
                    getTextAreaCode(1, '') + 
                '</td>' +
                '<td style="text-align: center; vertical-align: top">' +
                    choiceCode +
                '</td>' +
                '<td>' +
                '</td>' +
            '</tr>' +
        '</table>';
    
    if (isNull) {
        element.innerHTML += 
            '<p>' +
                '<button class="button" id="addResponseOptions" type="button" onclick="addNewRow(\'answerTable\')"> Add new response option </button>' +
            '</p>';
    }
}

function getTextAreaCode(rowsCount, id) {
    return '<textarea class="textarea input-validation-error" cols="60" data-val="true" ' +
                     'data-val-length="The field Answer must be a string with a maximum length of 256." ' +
                     'data-val-length-max="256" data-val-minlength="The field Answer must be a string or array type with a minimum length of \'1\'." ' +
                     'data-val-minlength-min="1" data-val-required="Response option text is required." ' +
                     'maxlength="256" name="AnswerOption.Answer" aria-describedby="answerOption-error AnswerOption.Answer-error" ' + id +
                     `aria-invalid="true" rows=${rowsCount}>` +
           '</textarea>';
}

function generateCorrectAnswerCode() {
    document.getElementById("answerInfo").innerHTML =
        '<label id="correctAnswerLabel">Correct answer*</label>' +
        '<p>' +
            getTextAreaCode(2, 'correctAnswer') +
        '</p>';
}
