
// new validator method
/*
jQuery.validator.addMethod(
	"ws:ce",
	function(value, element) {
		return this.optional(element) || value.indexOf("ciccio")!=-1//new RegExp("[\\d]").test(value);
	},
	"ws:ce must contains with 'ciccio'"
);
*/


// no need the "require" rule, it's automatically
// added if the parameter is mandatory (info taken from template)
/*
OpensearchFormIt.extensions["ws:ce"] = {
	rule: {
		"ws:ce": true
	},
}
*/

//var $geoGeom = $("<div><span>ciccio</span><br/><button class='btn btn-mini'>click</button></div>")
//	.css({
//		"background-color": "lightgrey",
//		padding: "5px",
//		border: "1px solid black"
//	});
//$geoGeom.val("ciccio");
//$geoGeom.find("button").click(function(){
//	$geoGeom.val(($geoGeom.val()=="ciccio" ? "ceras" : "ciccio"));
//	$geoGeom.find("span").text($geoGeom.val());
//	return false;
//});
//
OpensearchFormIt.extensions["geo:geometry"] = {
	field: $("<textarea></textarea>"),
//	rule: {
//		maxlength: 8
//	}
}

OpensearchFormIt.extensions["time:start"] = {
	field: '<input type="text" class="dateInput" />',
}
OpensearchFormIt.extensions["time:end"] = {
	field: '<input type="text" class="dateInput" />',
}




