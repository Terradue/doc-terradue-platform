/*
#
# Jquery-Twitter PAGINATION
# 
# By Ceras
# 
# Contact 	francesco.cerasuolo@terradue.com
*/

;(function($){
	
	$.fn.pagination = function(options){
		if (options==null)
			return;
		
//		totalResult: json.total_count,
//		startIndex: json.offset,
//		itemsPerPage: itemsPerPage,
//		changePage: function(infoPage){
//			alert("pageNumber:"+infoPage.getPageNumber+"\n" +
//					"startIndex:" +
//					infoPage.startIndex +
//					"\nitemsPerPage:" +
//					infoPage.itemsPerPage)
//
		var			
			totalResults = options.totalResults,
			startIndex = options.startIndex,
			indexOffset = (options.indexOffset ? parseInt(options.indexOffset) : 0),
			itemsPerPage = options.itemsPerPage,
			changePage = options.changePage,
			clazz = (options.clazz ? " "+options.clazz : ""),
			
			pageNumber = Math.floor((startIndex-indexOffset)/itemsPerPage),
			
			$div = $(this);
	
//		addVar("totalResults", totalResults);
//		addVar("startIndex", startIndex);
//		addVar("pageNumber", pageNumber);
//		addVar("itemsPerPage", itemsPerPage);
		
		var totalPages = Math.ceil(totalResults/itemsPerPage);
//		addVar("\ntotalPages", totalPages);

		if (totalPages<=1)
			return;

		var hasNext = (pageNumber+1 < totalPages);
		var has2Next = (pageNumber+2 < totalPages);
		var hasLast = (pageNumber+3 < totalPages)
		var hasNextDots = (pageNumber+4 < totalPages);
		
		var hasPrev = (pageNumber>0);
		var has2Prev = (pageNumber-1 > 0);
		var hasFirst = (pageNumber-2 > 0);
		var hasPrevDots = (pageNumber-3 > 0);
		
//		addVar("hasPrev", hasPrev);
//		addVar("hasFirst", hasFirst);
//		addVar("hasPrevDots", hasPrevDots);
//		addVar("has2Prev", has2Prev);
//	
//		addVar("\nhas2next", has2Next);
//		addVar("hasNextDots", hasNextDots);
//		addVar("hasLast", hasLast);
//		addVar("hasNext", hasNext);
//		
//		showVar();
		
		var html = "";
		html+= "<ul>";
		var $pagination = $("<ul>");
		
		if (hasPrev)
			$pagination.append($.linkToPage("Prev", pageNumber-1, options));
			
		if (hasFirst)
			$pagination.append($.linkToPage("1", 0, options));
		
		if (hasPrevDots)
			$pagination.append($("<li><a href=\"javascript://\">...</a></li>"));
			
		if (has2Prev)
			$pagination.append($.linkToPage(pageNumber-1, pageNumber-2, options));
			
		if (hasPrev)
			$pagination.append($.linkToPage(pageNumber, pageNumber-1, options));
		
		$pagination.append($("<li class=\"active\"><a href=\"#\">"+(pageNumber+1)+"</a></li>"));
	
		if (hasNext)
			$pagination.append($.linkToPage(pageNumber+2, pageNumber+1, options));
	
		if (has2Next)
			$pagination.append($.linkToPage(pageNumber+3, pageNumber+2, options));
	
		if (hasNextDots)
			$pagination.append($("<li><a href=\"#\">...</a></li>"));
			
		if (hasLast)
			$pagination.append($.linkToPage(totalPages, totalPages-1, options));
		
		if (hasNext)
			$pagination.append($.linkToPage("Next", pageNumber+1, options));
	
		$div.append($("<div class='pagination"+clazz+"' style='text-align: center'>").append($pagination));
	};
	
	
	$.linkToPage = function(text, pageNumber, options){
		var $a = $("<a href='#'>"+text+"</a>"),
			offset = pageNumber*options.itemsPerPage + parseInt(options.indexOffset),
			infoPage = {
				pageNumber: pageNumber,
				startIndex: offset,
				itemsPerPage: options.itemsPerPage,
			};
		
		$a.click(function(){
			options.changePage(infoPage);
			return false;
		});
		return $("<li>").append($a);
	}
})(jQuery);



function createPagination(divPagination, totalResults, startIndex, startPage, itemsPerPage, query) {
	
	/*
	totalResults = 123;
	startIndex = 0;
	startPage = 3;
	itemsPerPage = 10;
	// */
	
	addVar("totalResults", totalResults);
	addVar("startIndex", startIndex);
	addVar("startPage", startPage);
	addVar("itemsPerPage", itemsPerPage);
	
	var totalPages = Math.ceil(totalResults/itemsPerPage);
	addVar("\ntotalPages", totalPages);
	
	if (totalPages>1) {
		var hasNext = (startPage+1 < totalPages);
		var has2Next = (startPage+2 < totalPages);
		var hasLast = (startPage+3 < totalPages)
		var hasNextDots = (startPage+4 < totalPages);
		
		var hasPrev = (startPage>0);
		var has2Prev = (startPage-1 > 0);
		var hasFirst = (startPage-2 > 0);
		var hasPrevDots = (startPage-3 > 0);
	
		/*
		addVar("hasPrev", hasPrev);
		addVar("hasFirst", hasFirst);
		addVar("hasPrevDots", hasPrevDots);
		addVar("has2Prev", has2Prev);
	
		addVar("\nhas2next", has2Next);
		addVar("hasNextDots", hasNextDots);
		addVar("hasLast", hasLast);
		addVar("hasNext", hasNext);
		
		showVar();*/
		
		var html = "";
		html+= "<ul>";
		var url ="?" 
			+ (query==null ? "" : "q="+query +"&")
			+ "count="+itemsPerPage + "&page=";
		
		if (hasPrev)
			html+= "<li><a href=\"" +url+(startPage-1) + "\">Prev</a></li>";
			
		if (hasFirst)
			html+= "<li><a href=\"" +url+ "0\">1</a></li>";
		
		if (hasPrevDots)
			html+= "<li><a href=\"#\">...</a></li>";
			
		if (has2Prev)
			html+= "<li><a href=\"" +url+(startPage-2) + "\">" + (startPage-1) + "</a></li>";
			
		if (hasPrev)
			html+= "<li><a href=\"" +url+(startPage-1) + "\">" + (startPage) + "</a></li>";
			
		html+= "<li class=\"active\"><a href=\"#\">"+(startPage+1)+"</a></li>";
	
		if (hasNext)
			html+= "<li><a href=\"" +url+(startPage+1) + "\">" + (startPage+2) + "</a></li>";
	
		if (has2Next)
			html+= "<li><a href=\"" +url+(startPage+2) + "\">" + (startPage+3) + "</a></li>";
	
		if (hasNextDots)
			html+= "<li><a href=\"#\">...</a></li>";
			
		if (hasLast)
			html+= "<li><a href=\"" +url+(totalPages-1) + "\">" + (totalPages) + "</a></li>";
		
		if (hasNext)
			html+= "<li><a href=\"" +url+(startPage+1) + "\">Next</a></li>";
	
		html+= "</ul>";
		
		$(divPagination).html(html);
	}
	/*
	<ul>
		<li><a href="#">Prev</a></li>
		<li class="active"><a href="#">1</a></li>
		<li><a href="#">2</a></li>
		<li><a href="#">3</a></li>
		<li><a href="#">4</a></li>
		<li><a href="#">...</a></li>
		<li><a href="#">16</a></li>
		<li><a href="#">Next</a></li>
	</ul>*/
}

strVars = "";

function addVar(name, value) {
	strVars += "\n" + name + " = " + value;
}

function showVar() {
	alert("VARIABLES"+strVars);
	strVars = "";
}