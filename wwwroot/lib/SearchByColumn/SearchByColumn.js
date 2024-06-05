//for ColumnAdd
var id_count = 1;
var countRemove = 1;
var columnsSelected = new Array;
var oldColumn;

const columns = [
    ['Product_ID', 'number']
    , ['Postalcode', 'text']
    , ['Oeconym', 'text']
    , ['Byname', 'text']
    , ['Geography', 'text']
    , ['ParentCity', 'text']
    , ['Manufactory', 'text']
    , ['ProductionFacility', 'text']
    //, ['SerialNumber', 'text']
    //, ['ProductionYear', 'number']
    //, ['AAName', 'text']
    //, ['EraLong', 'text']
    //, ['Aerial', 'checkbox']
    //, ['ConditionOfCard', 'multiselect']
    //, ['DateInText', 'date']
    //, ['FullScreen', 'checkbox']
    //, ['Passepartout', 'checkbox']
    //, ['ColorProcessing', 'multiselect']
    //, ['ColorImage', 'text']
    //, ['ImageYear', 'number']
    //, ['ManufactoryName', 'text']
    //, ['Name', 'text']
    //, ['ThreePictures', 'checkbox']
    //, ['FourPictures', 'checkbox']
    //, ['MultiPictures', 'checkbox']
    //, ['Hoax', 'checkbox']
    //, ['Graduation', 'checkbox']
    //, ['Occasion', 'checkbox']
    //, ['Moon', 'checkbox']
    //, ['StudenticaColeur', 'checkbox']
    //, ['ImagePerception', 'multiselect']
    //, ['OfficialBusiness', 'checkbox']
    //, ['CorrugatedEdge', 'checkbox']
    //, ['ColorRALWritingFrontside', 'text']
    //, ['ColorRALPrintingBackside', 'text']
    //, ['Fieldpost', 'checkbox']
    //, ['Formats', 'multiselect']
    //, ['CardType', 'multiselect']
    //, ['CardSeries', 'number']
    //, ['Leporello', 'checkbox']
    //, ['Propaganda', 'checkbox']
    //, ['Ornament', 'checkbox']
    //, ['Technique', 'multiselect']
    //, ['Style', 'multiselect']
    //, ['UserName', 'text']
];

$('body').on('click', '.FieldAdd', function () { addField($(this).parent().prop('id').split('_').pop()) });
$('body').on('click', '.FieldRemove', removeField);
$('body').on('click', '.addColumn', AddColumn);
$('body').on('click', '.removeColumn', removeColumn);
$('body').on('click', '.ColumnDropDown', SelectedColumns)
$('body').on('change', '.ColumnDropDown', ChooseColumn);
$('body').on('click', '.AddPeriod', function () { addField($(this).parent().prop('id').split('_').pop()) });

if (window.location.href.indexOf('/AdministerCollection') > -1) {
    let columnNo = 0;
    for (let i = 0; i < columns.length; i++) {
        if (sessionStorage.getItem(columns[i][0]) != null) {
            SessionStorageGetInput(i, columnNo);
            columnNo++;
        }
    }
    if (columnNo == 0) {
        $('#form-holder_0').find('.ColumnDropDown').val('Choose_Column');
    }
};
if (window.location.href.indexOf('Detail') > -1) {
    let columnNo = 0;
    for (let i = 0; i < columns.length; i++) {
        if (sessionStorage.getItem(columns[i][0]) != null) {
            SessionStorageGetInputIntoDetail(i, columnNo);
            columnNo++;
        }
    }
};

function SessionStorageGetInputIntoDetail(index, columnNo) {
    let localKey = columns[index][0]
    let localValue = sessionStorage.getItem(columns[index][0]);
    let array = localValue.split(';');
    let type = columns[index][1];
    let inputNo = 0;

    if (columnNo > 0) {
        let source = $('#form-holder_0'), clone = source.clone(true);

        clone.attr('id', 'form-holder_' + id_count);
        clone.find('.multiselect').remove();
        clone.find('.new_chq').find('div[class*=inputDiv]').remove();
        clone.find('.new_chq').attr('id', 'newChq_' + id_count);
        clone.find('.total_input').attr('id', 'total_input' + id_count).val(1);

        clone.appendTo('.form-holder-append');
        id_count++;
        countRemove++;
    }

    switch (type) {
        case 'multiselect':
            createListBox(localKey, columnNo);
            array.forEach(function (item) {
                $('[value="' + item + '"]').prop('checked', true);
            });
            break;
        case 'checkbox':
            createInput(localKey, 'checkbox', columnNo);
            $('#inputBox_' + columnNo + inputNo).prop('checked', true);
            break;
        default:
            array.forEach(function (item) {
                if (item != '') {
                    createInput(localKey, type, columnNo);
                    $('#inputBox_' + columnNo + inputNo).val(item);
                }
            });
            break;
    }
}

function SessionStorageGetInput(index, columnNo) {
    let type = columns[index][1]
    let localKey = columns[index][0]
    
    let localValue = sessionStorage.getItem(localKey)
    let array = localValue.split(';')
    let inputNo = 0

    if (columnNo > 0)
        AddColumn();       
    $('#columnName_' + columnNo).val(localKey);

    switch (type) {
        case 'multiselect':
            createListBox(localKey, columnNo);
            array.forEach(function (item) {
                $('[value="' + item + '"]').prop('checked', true);
            });
            break;
        case 'checkbox':
            createInput(localKey, 'checkbox', columnNo);
            $('#inputBox_' + columnNo + inputNo).prop('checked', true);
            break;
        default:
            array.forEach(function (item) {
                if (item != '') {
                    if (inputNo == 0)
                        createInput(localKey, type, columnNo);
                    else
                        addField(columnNo);
                    $('#inputBox_' + columnNo + inputNo).val(item);
                    inputNo++;
                    $('#total_input' + columnNo).val(inputNo);
                }
            });
            if (type == 'date' || type == 'number') {
                if (inputNo == 1)
                    createPeriodButton(columnNo);
            } else
                createAddButton(columnNo);
            break;
    }
}

function SelectedColumns() {
    oldColumn = $(this).children(':selected').val();
    $(this).children().show();

    for (let i = 0; i < columnsSelected.length; i++) {
        $(this).children('option[value="' + columnsSelected[i] + '"]').hide();
    }
    $(this).children('option[value="Choose_Column"]').hide();
}

function ChooseColumn() {
    let parentId = $(this).parent().parent().prop('id').split('_').pop()
    let column = $(this).children(':selected').val()
    let index = 0

    if(oldColumn != 'Choose_Column')
        columnsSelected.splice(columnsSelected.indexOf(oldColumn), 1);
    columnsSelected.push(column);

    $('.inputDiv_' + parentId).remove();
    $('#btnPeriod_' + parentId).remove();
    $('#multiselect_' + parentId).remove();
    $('#removeField_' + parentId).remove();
    $('#addField_' + parentId).remove();
    $('#total_input' + parentId).val(1);

    for (let i = 0; i < columns.length; i++) {
        if (columns[i][0] == column) {
            index = i;
            i = columns.length - 1;
        }
    }
    let type = columns[index][1];
    switch (type) {
        case 'multiselect':
            createListBox(column, parentId);
            break;
        case 'year':
        case 'date':
        case 'number':
            createInput(column, type, parentId);
            createPeriodButton(parentId);
            break;
        case 'checkbox':
            createInput(column, 'checkbox', parentId);
            break;
        default:
            createInput(column, type, parentId);
            //folgende 2 Zeilen notwendig wegen autocomplete
            addField(parentId);
            $('#inputDiv_' + parentId + '0').hide();
            createAddButton(parentId);
            break;
    }
}

function createInput(columnName, columnType, parentIdInput) {
    let new_chq = document.getElementById('newChq_' + parentIdInput);
    let elementDiv = document.createElement('DIV');
    let elementInput = document.createElement("INPUT");

    elementDiv.setAttribute('class', 'input-group inputDiv_' + parentIdInput);
    elementDiv.setAttribute('id', 'inputDiv_' + parentIdInput + '0');
    new_chq.appendChild(elementDiv);
    $('#DivAdd_' + parentIdInput).insertAfter(elementDiv);

    elementInput.setAttribute("id", "inputBox_" + parentIdInput + '0');
    elementInput.setAttribute("name", 'Search' + columnName);

    switch (columnType) {
        case 'text':
            elementInput.setAttribute("type", "text");
            elementInput.setAttribute("class", "SearchByColumnField form-control");
            break;
        case 'year':
            elementInput.setAttribute("type", "number");
            elementInput.setAttribute("min", "1700");
            elementInput.setAttribute("max", "2100");
            elementInput.setAttribute("class", "SearchByColumnField form-control");
            break;
        case 'number':
            elementInput.setAttribute("type", "number");
            elementInput.setAttribute("class", "SearchByColumnField form-control");
            break;
        case 'date':
            elementInput.setAttribute("type", "date");
            elementInput.setAttribute("class", "SearchByColumnField form-control");
            break;
        case 'checkbox':
            elementInput.setAttribute("type", "checkbox");
            elementInput.setAttribute("class", "SearchByColumnField form-check-input");
            break;
        default:
    }

    elementDiv.appendChild(elementInput);
}

function createListBox(colName, parentId) {
    let new_chq = document.getElementById('newChq_' + parentId);

    let newListBox = document.createElement("table");
    newListBox.setAttribute('id', 'multiselect_' + parentId);
    newListBox.setAttribute('class', 'multiselect');

    if (colName == 'ConditionOfCard')
        newListBox.innerHTML = '<tr><td><input type="checkbox" name="SearchConditionOfCard" value="1"/></td><td><label>ungebraucht</label></td></tr><tr><td><input type="checkbox" name="SearchConditionOfCard" value="2"/></td><td><label>beschrieben, aber nicht gelaufen</label></td></tr><tr><td><input type="checkbox" name="SearchConditionOfCard" value="3"/></td><td><label>gelaufen mit Marke</label></td></tr><tr><td><input type="checkbox" name="SearchConditionOfCard" value="4"/></td><td><label>gelaufen, aber Marke entfernt</label></td></tr><tr><td><input type="checkbox" name="SearchConditionOfCard" value="5"/></td><td><label>repariert</label></td></tr>'
    else if (colName == 'Formats')
        newListBox.innerHTML = '<tr><td><input type="checkbox" name="SearchFormats" value="1"/></td><td><label>Format klein T74</label></td></tr><tr><td><input type="checkbox" name="SearchFormats" value="2"/></td><td><label>Format groß T76</label></td></tr ><tr><td><input type="checkbox" name="SearchFormats" value="3"/></td><td><label>Format Übergroß</label></td></tr>'
    else if (colName == 'CardType')
        newListBox.innerHTML = '<tr><td><input type="checkbox" name="SearchCardType" value="1"/></td><td><label>Ansichtskarte</label></td></tr><tr><td><input type="checkbox" name="SearchCardType" value="2"/></td><td><label>Postkarte</label></td></tr>'
    else if (colName == 'ImagePerception')
        newListBox.innerHTML = '<tr><td><input type="checkbox" name="SearchImagePerception" value="1"/></td><td><label>Ausschließlich Grafik</label></td></tr><tr><td><input type="checkbox" name="SearchImagePerception" value="2"/></td><td><label>Fotografie</label></td></tr><tr><td><input type="checkbox" name="SearchImagePerception" value="3"/></td><td><label>Zeichnung</label></td></tr><tr><td><input type="checkbox" name="SearchImagePerception" value="4"/></td><td><label>Gemälde</label></td></tr>'
    else if (colName == 'ColorProcessing')
        newListBox.innerHTML = '<tr><td><input type="checkbox" name="SearchColorProcessing" value="1"/></td><td><label>schwarz/weiß</label></td></tr><tr><td><input type="checkbox" name="SearchColorProcessing" value="2"/></td><td><label>einfarbing</label></td></tr><tr><td><input type="checkbox" name="SearchColorProcessing" value="3"/></td><td><label>mehrfarbig</label></td></tr>'
    else if (colName == 'Technique')
        newListBox.innerHTML = '<tr><td><input type="checkbox" name="SearchTechnique" value="1"/></td><td><label>Hochdruck</label></td></tr><tr><td><input type="checkbox" name="SearchTechnique" value="2"/></td><td><label>Flachdruck</label></td></tr><tr><td><input type="checkbox" name="SearchTechnique" value="3"/></td><td><label>Durchdruck</label></td></tr><tr><td><input type="checkbox" name="SearchTechnique" value="4"/></td><td><label>Tiefdruck</label></td></tr><tr><td><input type="checkbox" name="SearchTechnique" value="4"/></td><td><label>Digitaldruck</label></td></tr>'
    else if (colName == 'Style') 
        newListBox.innerHTML = '<tr><td><input type="checkbox" name="SearchStyle" value="1"/></td><td><label>Buchdruck/Letterpress (Hochdruck)</label></td></tr><tr><td><input type="checkbox" name="SearchStyle" value="2"/></td><td><label>Flexodruck (Hochdruck)</label></td></tr><tr><td><input type="checkbox" name="SearchStyle" value="3"/></td><td><label>Linolschnitt/Holzschnitt (Hochdruck)</label></td></tr><tr><td><input type="checkbox" name="SearchStyle" value="4"/></td><td><label>Steindruck (Lithographie, Flachdruck)</label></td></tr><tr><td><input type="checkbox" name="SearchStyle" value="5"/></td><td><label>Offsetdruck (Flachdruck)</label></td></tr><tr><td><input type="checkbox" name="SearchStyle" value="6"/></td><td><label>Lichtdruck (Flachdruck)</label></td></tr><tr><td><input type="checkbox" name="SearchStyle" value="7"/></td><td><label>Siebdruck (Durchdruck)</label></td></tr><tr><td><input type="checkbox" name="SearchStyle" value="8"/></td><td><label>Risographie (Durchdruck)</label></td></tr><tr><td><input type="checkbox" name="SearchStyle" value="9"/></td><td><label>Schablonendruck (Durchdruck)</label></td></tr><tr><td><input type="checkbox" name="SearchStyle" value="10"/></td><td><label>Tampondruck (Tiefdruck)</label></td></tr><tr><td><input type="checkbox" name="SearchStyle" value="11"/></td><td><label>Radierung (Tiefdruck)</label></td></tr><tr><td><input type="checkbox" name="SearchStyle" value="12"/></td><td><label>Schabtechnik (Tiefdruck)</label></td></tr><tr><td><input type="checkbox" name="SearchStyle" value="13"/></td><td><label>Kupfer-/Stahlstich (Tiefdruck)</label></td></tr>'
    new_chq.appendChild(newListBox);
}

function createAddButton(idParent) {
    let addbutton = document.createElement("Button");
    addbutton.setAttribute('class', 'btn btn-outline-success start-0 FieldAdd');
    addbutton.setAttribute('type', 'button');
    addbutton.setAttribute('id', 'addField_' + idParent);
    addbutton.innerHTML = '+';

    document.getElementById('DivAdd_' + idParent).appendChild(addbutton);
}

function createPeriodButton(idParent) {
    let periodButton = document.createElement("Button");
    periodButton.setAttribute('class', 'btn btn-outline-info btn-sm start-0 AddPeriod');
    periodButton.setAttribute('id', 'btnPeriod_' + idParent);
    periodButton.setAttribute('type', 'button');
    periodButton.innerHTML = 'Zu Zahlenraum ändern';

    document.getElementById('DivAdd_' + idParent).appendChild(periodButton);
}

function createRemoveButton(idParent) {
    let RemoveButton = document.createElement("Button");
    RemoveButton.setAttribute('class', 'btn btn-outline-danger end-0 FieldRemove');
    RemoveButton.setAttribute('type', 'button');
    RemoveButton.setAttribute('id', 'removeField_' + idParent);
    RemoveButton.innerHTML = '-';

    document.getElementById('inputDiv_' + idParent).appendChild(RemoveButton);
    $('#removeField_' + idParent).insertAfter('#inputBox_' + idParent);
}

function addField(IdParent) {
    let last_input_count = parseInt($('#total_input' + IdParent).val());
    let new_input_count = parseInt($('#total_input' + IdParent).val()) + 1;
    let sourceDiv = $('#inputDiv_' + IdParent + '0'), cloneDiv = sourceDiv.clone(true);

    cloneDiv.attr('id', 'inputDiv_' + IdParent + last_input_count).show();
    cloneDiv.appendTo('#newChq_' + IdParent);
    cloneDiv.find('#inputBox_' + IdParent + '0').attr('id', 'inputBox_' + IdParent + last_input_count).val('');
    cloneDiv.find('#removeField_' + IdParent + '0').attr('id', 'removeField_' + IdParent + last_input_count);

    if (new_input_count ==2 ) {
        createRemoveButton(IdParent.toString() + last_input_count.toString());
        createRemoveButton(IdParent + '0');
    }
    $('#btnPeriod_' + IdParent).remove();
    $('#DivAdd_' + IdParent).insertAfter(cloneDiv);

    $('#total_input' + IdParent).val(new_input_count);
}

function removeField() {
    let classParent = $(this).parent().prop('class').split('_').pop();
    let last_input_count = $('#total_input' + classParent).val() - 1;
    let type = $(this).parent().parent().find('input').attr('type');
    let closest = $(this).closest('div');
    let closestId = closest.prop('id');

    closest.remove();
    if (closestId == 'inputDiv_' + classParent + '0') {
        let newDiv0 = $('#newChq_' + classParent).find('div:first');
        newDiv0.find('.FieldRemove').attr('id', 'removeField_' + classParent + '0');
        newDiv0.find('input').attr('id', 'inputBox_' + classParent + '0');
        newDiv0.attr('id', 'inputDiv_' + classParent + '0');
    }

    $('#total_input' + classParent).val(last_input_count);

    if (type == 'number' || type == 'date')
        createPeriodButton(classParent);

    if (last_input_count == 1)
        $('#removeField_' + classParent + '0').remove();
}

function AddColumn() {
    let source = $('#form-holder_0'), clone = source.clone(true);

    clone.attr('id', 'form-holder_' + id_count);
    clone.find('.ColumnDropDown').attr('id', 'columnName_' + id_count).val('Choose_Column');
    clone.find('.removeColumn').attr('id', 'removeColumn_' + id_count).show();
    clone.find('#DivAdd_0').attr('id', 'DivAdd_' + id_count);
    clone.find('.multiselect').remove();
    clone.find('.new_chq').find('div[class*=inputDiv]').remove();
    clone.find('.FieldAdd').remove();
    clone.find('.FieldRemove').remove();
    clone.find('.AddPeriod').remove();
    clone.find('.new_chq').attr('id', 'newChq_' + id_count);
    clone.find('.total_input').attr('id', 'total_input' + id_count).val(1);

    clone.appendTo('.form-holder-append');
    id_count++;
    countRemove++;

    $('#removeColumn_0').show();
}

// Removing Form Field
function removeColumn() {
    let closest = $(this).closest('div').parent();
    let closestId = closest.prop('id');
    let column = $(this).parent().parent().find('.ColumnDropDown').children(':selected').val();

    columnsSelected.splice(columnsSelected.indexOf(column), 1);
    closest.remove();

    if (closestId == 'form-holder_0') {
        let newformholder0 = $('.SbCDiv:first');
        let oldFormHolderId = newformholder0.prop('id').split('_').pop();
        newformholder0.find('.removeColumn').attr('id', 'removeColumn_0');
        newformholder0.find('.ColumnDropDown').attr('id', 'columnName_0');
        newformholder0.find('.DivAdd').attr('id', 'DivAdd_0');
        newformholder0.find('.FieldAdd').attr('id', 'addField_0');
        newformholder0.find('.AddPeriod').attr('id', 'btnPeriod_0');
        newformholder0.find('.new_chq').attr('id', 'newChq_0');
        newformholder0.find('.total_input').attr('id', 'total_input0');
        newformholder0.find('.multiselect').attr('id', 'multiselect_0');

        let InputElements = newformholder0.find('#newChq_0').find('input');
        for (i = 0; i < InputElements.length; i++) {
            newformholder0.find('#newChq_0').find('div#inputDiv_' + oldFormHolderId + i).attr('id', 'inputDiv_0' + i).attr('class', 'input-group inputDiv_0');
            newformholder0.find('#newChq_0').find('input#inputBox_' + oldFormHolderId + i).attr('id', 'inputBox_0' + i);
            newformholder0.find('#newChq_0').find('button#removeField_' + oldFormHolderId + i).attr('id', 'removeField_0' + i);
        }
        newformholder0.attr('id', 'form-holder_0');
    }
    countRemove--;
    if (countRemove == 1)
        $('#removeColumn_0').hide();
}

function SetSessionStorageData() {
    $(".submitButton").hide();
    $(".spinnerButton").show();

    sessionStorage.clear();

    for (let i = 0; i < columns.length; i++) {
        let type = columns[i][1];
        let column = columns[i][0];
        switch (type) {
            case 'checkbox':
                SessionStorageFillCheckBox(column);
                break;
            case 'multiselect':
                SessionStorageFillListBox(column);
                break;
            default:
                SessionStorageFillInput(column);
        }
         
    }
}

function SessionStorageFillInput(columnName) {
    let ItemsColumn = '';

    $('.SearchByColumnField[name="Search' + columnName + '"]').each(function () {
        if ($(this).val() != '') {
            if (ItemsColumn != '')
                ItemsColumn = ItemsColumn + $(this).val() + ";";
            else
                ItemsColumn = $(this).val() + ";";
        }
    })
    if (ItemsColumn != '')
        sessionStorage.setItem(columnName, ItemsColumn);
}

function SessionStorageFillListBox(columnName) {
    let ItemsColumn = '';

    $('[name="Search' + columnName + '"]:checked').each(function () {
        if ($(this).val() != '') {
            ItemsColumn += $(this).val() + ";";
        }
    })
    if (ItemsColumn != '')
        sessionStorage.setItem(columnName, ItemsColumn);
}

function SessionStorageFillCheckBox(columnName) {
    let itemsColumn = $('input.SearchByColumnField[name="Search' + columnName + '"]:checked').val();
    if (itemsColumn != null)
        sessionStorage.setItem(columnName, itemsColumn)
}

function searchReset() {
    window.sessionStorage.clear();
    for (let i = 1; i < id_count; i++) {
        $('#form-holder_' + i).remove();
    }

    $('#form-holder_0').find('.multiselect').remove();
    $('#form-holder_0').find('.new_chq').find('.inputDiv_0').remove();
    $('#form-holder_0').find('.FieldAdd').remove();
    $('#form-holder_0').find('.AddPeriod').remove();
    $('#form-holder_0').find('.total_input').val(1);
    $('#form-holder_0').find('.ColumnDropDown').val('Choose_Column');
    id_count = 1;
    columnsSelected = [];
}