/*
#
# Json Table - Jquery-Twitter table creator
# 
# By Ceras
# 
# Contact 	francesco.cerasuolo@terradue.com
*/

var JsonTable = {
	TYPE_STRING: "String",
	TYPE_DATE_TIME: "DateTime",
	MONTHS: ["January","February","March","April","May","June","July","August","September","October","November","December"],
	
	defaultOptions: {
		showHeader: true,
		columnsToShow: null,
		idColumn: null,
		columns: {},
		tableClass: "table table-striped table-hover",
		css: {},
		rowRenderer: function($tr, jsonRow){},
		highlightRow: function($tr, jsonRow){
			$tr.addClass("highlight");
		},
		unhighlightRow: function($tr){
			$tr.removeClass("highlight");
		},
		onRowClick: null, // it can be a function(id, json) if idColumn is defined, function(json) otherwise 
		onRowMouseover: null,
		onRowMouseout: null,
		showFilter: null, // boolean
		filterInput: null, // jquery selector (or id, or element) of input text used to search
						   // if null the input text is prepend to the table
		filterLimit: 5, // lower limit to show filter input 
	},
}

;(function($){

	var noDataToDisplay = function($div, options) {
		if (options.noDataToDisplay == null)
			return $("<span>No data to display.</span>");
		else if (typeof(options.noDataToDisplay)=="function")
			return options.noDataToDisplay($div);
		else
			return $("<span>").append(options.noDataToDisplay);
	};

	var renderCell = function(jsonRow, index, columnName, value, options) {
		// manage case of complex column name
		// es. "position.city.name"
		if (value==null){
			try {
				value = eval("jsonRow."+columnName);
			} catch(err) {
				value = "";
			}
		}

		var columns = options.columns;
		if (columns[columnName]==null)
			return stringRenderer(value); // default case
		
		var renderer = columns[columnName].renderer;
		if (renderer!=null)
			return renderer(jsonRow, index, columnName, value);
			
		var type = columns[columnName].type;
		switch(type) {
		case JsonTable.TYPE_DATE_TIME: return dateTimeRenderer(value); break
		default: return stringRenderer(value);
		}
	}
	
	// DEFAULT RENDERERS
	var stringRenderer = function(value) {
		if (typeof(value)=="function")
			return "";
		return value;
	}
	var dateTimeRenderer = function(value) {
		if (value==null || value=="")
			return "";
		var d = new Date(parseInt(value.substr(6)));
		return "<i>" + (d.getDate()) + " " + JsonTable.MONTHS[d.getMonth()] + " " + d.getFullYear() + "</i>";
	}
	
	function _addRow(jsonRow, index, $table, $tbody, $noDataToDisplay, options, columnNames, indexTable){
		var $tr = $("<tr>");
		if (options.idColumn){
			var id = (indexTable==null ? jsonRow : jsonRow[options.idColumn]);
			if (typeof(id=="string"))
				$tr.attr("data-id", id);
		}
		
		options.rowRenderer($tr, jsonRow);
		for (var i in columnNames ) {
			var colName = columnNames[i],
				$td = $("<td>").append(renderCell(jsonRow, index, colName, jsonRow[colName], options));
			if (options.columns[colName]!=null && options.columns[colName].css!=null){
				var css = options.columns[colName].css;
				if (typeof(css) == "function")
					$td.css(css(jsonRow, index, colName, jsonRow[colName], options));
				else
					$td.css(css);
			}
			$tr.append($td);
		}
		$tbody.append($tr);

		// events click, mouseover and mouseout
		if (options.onRowClick){
			$tr.addClass("link");
			$tr.click(function(ev){
				if (options.idColumn)					
					return options.onRowClick(id, jsonRow, $tr, ev);
			});
		}
		if (options.onRowMouseover)
			$tr.mouseover(function(){
				if (options.idColumn)
					options.onRowMouseover(id, jsonRow, $tr);
			});
		if (options.onRowMouseout)
			$tr.mouseout(function(){
				if (options.idColumn)
					options.onRowMouseout(id, jsonRow, $tr);
			});
		
		// add element to index
		if (options.idColumn && typeof(id=="string"))
			indexTable[id] = { data: jsonRow, $tr: $tr };
		
		$noDataToDisplay.hide();
		$table.show();
	}
	
	$.fn.jsonTable = function(json, options){

		if (json==null)
			return this;
		
		var $div = $(this), self=this,
			options = $.extend({}, JsonTable.defaultOptions, options);
		
		$noDataToDisplay = noDataToDisplay($div, options).hide();
		
		// indexing (if idColumn!=null)
		if (options.idColumn)
			this.indexTable = {};
		
		// retrieve column names
		var columnNames = null;
		// try to retrieve columns by columnsToShow parameter
		if (options.columnsToShow!=null)
			columnNames = options.columnsToShow;
		else {
			// read the first row to get columns
			var firstRow = json[0];
			var columnNames = new Array();
			for (var key in firstRow)
				columnNames.push(key);
		}
		
		var $table = $("<table class='" + options.tableClass +  "'"
				+ (options.tableId==null? "" : " id='" +options.tableId + "'")
				+ ">").css(options.css);

		// create the table header from first row
		//if (options.showHeader){
			var $tr = $("<tr>");
			$table.append($("<thead>").append($tr));
			
			for (var i in columnNames ) {
				var colName = columnNames[i], w=null;
				if (options.columns[colName]!=null) {
					var opt = options.columns[colName];
					if (opt.displayedName!=null)
						colName = opt.displayedName;
					if (opt.width!=null)
						w = opt.width;
				}
				var $th = $("<th>");
					if (options.showHeader)
						$th.append(colName);
					else
						$th.css("padding", "0px");
				if (w!=null) $th.css("width", w);
				$tr.append($th);
			}		
		//}
		
		
		// add rows to the table
		var $tbody = $("<tbody>");
		$.each(json, function(index){
			_addRow(this, index, $table, $tbody, $noDataToDisplay, options, columnNames, self.indexTable);
		});
		$table.append($tbody);
		
		// filter input
		var $filterInput=null;
		if (options.showFilter && json.length>options.filterLimit){
			$filterInput = options.filterInput ? $(options.filterInput) : $("<input type='text' placeholder='Filter' />");
			$filterInput.keyup(function(){
				var val = $filterInput.val().toUpperCase();
				if (val==null || val==""){
					$table.find("tbody>tr").show();
					if (json.length>0){
						$noDataToDisplay.hide();
						$table.show();						
					}
				}
				else {
					var count=0;
					$table.find("tbody>tr").each(function(){
						var $tr = $(this), show=false;
						$tr.children("td").each(function(){
							if ($(this).text().toUpperCase().indexOf(val) != -1){
								show=true; return false;
							}	
						});
						if (show) {
							$tr.show(); count++;
						}else $tr.hide();
					});
					if (count>0) {
						$noDataToDisplay.hide();
						$table.show();						
					}
					else {
						$noDataToDisplay.show();
						$table.hide();						
					}
				}
			});
		}
		
		if (!options.filterInput)
			$div.append($filterInput);
		$div.append($noDataToDisplay, $table);
		
		// no rows case
		if (json.length==null || json.length==0) {
			$noDataToDisplay.show();
			$table.hide();
		}


		this.options=options;
		this.$table = $table;
		this.$tbody = $tbody;
		this.$noDataToDisplay = $noDataToDisplay;
		this.columnNames = columnNames;
		
		// function to highlight row
		this.highlight = function(id){
			var options = this.options;
			if (options.idColumn){
				var row = this.indexTable[id];
				if (row!=null){
					options.highlightRow(row.$tr, row.data);
					//row.$tr.css("border", "2px solid red");
				}
			}
			return this;
		};
		
		this.unhighlightAll = function(){
			var options = this.options;
			$.each(this.indexTable, function(){
				options.unhighlightRow(this.$tr);
			});
			return this;
		};
		
		this.getRow = function(id){
			if (options.idColumn){
				var row = this.indexTable[id];
				if (row)
					return row;
				else
					return null;
			} else
				return null;
		};
		
		this.getRows = function(ids){
			if (options.idColumn){
				var data = [], $tr=[], that=this;
				$.each(ids, function(){
					if (that.indexTable[this]){
						data.push(that.indexTable[this].data);
						$tr.push(that.indexTable[this].$tr);
					}
				});
				return {data: data, $tr: $tr};
			} else
				return null;
		};
		
		this.addRow = function(jsonRow){
			_addRow(jsonRow, 0, this.$table, this.$tbody, this.$noDataToDisplay, this.options, this.columnNames, this.indexTable)
		}
		
		return this;
	};
	
})(jQuery);
