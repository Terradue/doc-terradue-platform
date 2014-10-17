
// BY CERAS

L.Control.Expandible = L.Control.extend({
	
	options : {
		position : 'topleft',
		collapsedIcon: "icon-search",
		collapsedText: null, //"this is a text",
		collapsedContent: null, //"<i class='icon-search expandible-control-icon'></i>",
		expandedContent: "<div style='width:100px; height:100px;'>",
		disableMapScroll: false,
		resizable: false,
		overflow: "auto", // overflow for container of expandend content (expandend container)
		onExpand: null, // function,
		onCollapse: null,
	},
	
	onAdd : function(map) {
		console.log(this.options);
		var opt = this.options,
			collapseSymbol = ((opt.position=="topright" || opt.position=="bottomright") ? "&raquo;" : "&laquo;"),
			$container = $("<div class='expandible-control leaflet-control collapsed'>"),
			$collapsedContainer = $("<div class='expandible-control-collapsedContainer'>").appendTo($container),
			$collapseBtn = $("<p class='expandible-control-collapseBtn'>"+collapseSymbol+"</p>").hide().appendTo($container),
			$expandedContainer = $("<div class='expandible-control-expandedContainer'></div>")
				.hide().appendTo($container),
			$expandedContent = $(opt.expandedContent).appendTo($expandedContainer);
		
		if (this.options.resizable) {
			$expandedContainer.css({
				"bottom": "0px",
				"right": "0px",
			});
		}
		
		if (opt.collapsedText!=null)
			$collapsedContainer.append($("<p class='expandible-control-collapsedText'><nobr>"+opt.collapsedText+"</nobr></p>"));
		else if (opt.collapsedContent!=null)
			$collapsedContainer.append($(opt.collapsedContent));
		else if (opt.collapsedIcon!=null)
			$collapsedContainer.append($("<i class='expandible-control-collapsedIcon " + opt.collapsedIcon + "'></i>"));
			
		var div = $container.get(0),	
			stop = L.DomEvent.stopPropagation,
			that = this;

        L.DomEvent
            .on(div, 'click', stop)
            .on(div, 'mousedown', stop)
            .on(div, 'dblclick', stop);
        
        if (opt.disableMapScroll){        	
        	// problems on L.DomEvent.on(div, 'mousweel') stopPropagation
        	// solved with three event management        	
        	$expandedContainer.on("mousewheel", function(e){
        		//console.log("inner wheel chrome; ");
        		e.stopPropagation();
        	});
         	$expandedContainer.on("DOMMouseScroll", function(e){
         		//console.log("inner wheel ffox DOM; ");
        		e.stopPropagation();
        	});
         	$expandedContainer.on("MozMousePixelScroll", function(e){
         		//console.log("inner wheel ffox MOZ; ");
        		e.stopPropagation();
        	});
        }
        
		this.$container = $container;
		this.$collapsedContainer = $collapsedContainer;
		this.$expandedContainer = $expandedContainer.css("padding", this.options.padding);
		this.$expandedContent = $expandedContent;
		this.$collapseBtn = $collapseBtn;
		this.enabled = true;
		this.firstExpand = true;
		this.isCollapsed = true;

		$collapsedContainer.click(function(){
			if (that.enabled)
				that.expand();
		});
		
		$collapseBtn.click(function(){
			if (that.enabled)
				that.collapse();
		});
		
		//var w=$expandedContent.width(), h=$expandedContent.height();
		//this.exw=w; this.exh=h;
		$container.css({width: "", height:"", overflow: "inherit"});
		$expandedContainer.css({ overflow: this.options.overflow });
		//$expandedContent.css({width: "100%", height: "100%", padding: "0px"});
		//this.collapse();

		if (this.options.resizable) {
			//$container.width(w).height(h);
			$container.resizable();
			$container.find(".ui-resizable-handle").hide();
		}
		
		return div;
	},
	
	expand: function() {
		if (!this.isCollapsed)
			return this;
		
		if (this.exw!=null)
			var w = this.exw, h = this.exh;
		else if (this.options.resizable)
			var w = this.options.resizable.initialWidth, h = this.options.resizable.initialHeight;
		else
			var w = this.$expandedContainer.outerWidth()+2, h = this.$expandedContainer.outerHeight()+2; 

		this.$collapsedContainer.fadeOut();
		this.$expandedContainer.fadeIn();
		this.$collapseBtn.fadeIn();
		this.$container.removeClass("collapsed").animate({
			width: w, height: h,
		});
		if (this.options.resizable)
			this.$container.find(".ui-resizable-handle").show();
		this.$collapseBtn.show();
		this.isCollapsed = false;
		
		if (this.options.onExpand)
			this.options.onExpand(this);
		
		return this;
	},
	
	collapse: function() {
		if (this.isCollapsed)
			return this;
		
		var w = this.$collapsedContainer.width()+2,
			h = this.$collapsedContainer.height()+2;
		
		if (this.options.resizable || (!this.options.resizable && this.exw==null)) {
			this.exw = this.$container.width(),
			this.exh = this.$container.height();
		}
		
		this.$collapsedContainer.fadeIn();
		this.$expandedContainer.fadeOut();
		this.$collapseBtn.fadeOut();
		this.$container.addClass("collapsed").animate({
			width: w, height: h,
		});
		if (this.options.resizable)
			this.$container.find(".ui-resizable-handle").hide();
		this.isCollapsed = true;
		
		if (this.options.onCollapse)
			this.options.onCollapse(this);

		return this;
	},
	
	enable: function(){
		this.enabled = true;		
		this.$container.removeClass("disabled");
		return this;
	},
	
	disable: function(){
		this.enabled = false;
		this.$container.addClass("disabled");
		return this;
	},
	
});

		