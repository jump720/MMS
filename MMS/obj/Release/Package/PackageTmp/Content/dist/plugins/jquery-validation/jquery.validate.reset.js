$.validator.methods.min = function (value, element, param) {
    var globalizedValue = value.replace(",", ".");
    return this.optional(element) || globalizedValue >= param;
}

$.validator.methods.max = function (value, element, param) {
    var globalizedValue = value.replace(",", ".");
    return this.optional(element) || globalizedValue <= param;
}

$.validator.methods.range = function (value, element, param) {
    value = value.replace(",", ".");
    return this.optional(element) || (value >= param[0] && value <= param[1]);
}

$.validator.methods.number = function (value, element) {
    return this.optional(element) || /^-?(?:\d+)(?:[\.,]\d{1,2})?$/.test(value);
}

//$.validator.methods.number = function (value, element) {
//    return this.optional(element) || /^-?(?:\d+|\d{1,3}(?:[\s\.,]\d{3})+)(?:[\.,]\d+)?$/.test(value);
//}

$.validator.addMethod("regex", function (value, element, regexp) {
    var re = new RegExp(regexp);
    return this.optional(element) || re.test(value);
},
"Please check your input.");