<?xml version="1.0" encoding="iso-8859-1" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:output method="html" encoding="iso-8859-1" /> 
	<xsl:template name="print-defect-rules">
		<xsl:param name="name" />
		: <xsl:value-of select="count(//rule[@Name = $name]/target/defect)" /> defects
	</xsl:template>
	<xsl:template name="print-rules">
		<xsl:param name="type" />
			<p>
				<b><xsl:value-of select="$type" /></b>:
				<xsl:choose>
					<xsl:when test="count(rules/rule[@Type = $type]) = 0">
						<ul>
							<li>None</li>
						</ul>									
					</xsl:when>
					<xsl:otherwise>						
						<ul>
							<xsl:for-each select="rules/rule[@Type = $type]">
								<li>
									<a href="{@Uri}" target="{@Name}"><xsl:value-of select="text()" /></a>
									<xsl:call-template name="print-defect-rules">
										<xsl:with-param name="name">
											<xsl:value-of select="@Name" />
										</xsl:with-param>
									</xsl:call-template>
								</li>
							</xsl:for-each>
						</ul>						
					</xsl:otherwise>
				</xsl:choose>				
			</p>			
	</xsl:template>
	<xsl:template match="/">
		<xsl:for-each select="gendarme-output">
			<html>
				<head>
					<title>Gendarme Report</title>
				</head>
				<style type="text/css">
					h1, h2, h3 {
						font-family: Verdana;
						color: #68892F;
					}
					h2 {
						font-size: 14pt;
					}
					
					p, li, b {
						font-family: Verdana;
						font-size: 11pt;
					}			
					p.where, p.problem, p.found, p.solution {
						background-color: #F6F6F6;
						border: 1px solid #DDDDDD;
						padding: 10px;
					}
					span.found {
						margin-left: 10px;
					}
					div.toc {
						background-color: #F6F6F6;
						border: 1px solid #DDDDDD;
						padding: 10px;	
						float: right;				
						width: 300px;						
					}
					a:link, a:active, a:hover, a:visited {
						color: #9F75AD;
						font-weight: bold;
						text-decoration: none;
					}
				</style>
				<body>
					
					<h1>Gendarme Report</h1>
					<p>Produced on <xsl:value-of select="@date" /> UTC.</p>
					
					<div class="toc">
						<div align="center">
							<b style="font-size: 10pt;">Table of contents</b>
						</div>
						<p style="font-size: 10pt;">														
							<a href="#s1">1&#160;&#160;Summary</a><br />
							<a href="#s1_1">&#160;&#160;1.1&#160;&#160;List of assemblies searched</a><br />
							<a href="#s1_2">&#160;&#160;1.2&#160;&#160;List of rules used</a><br />
							<a href="#s2">2&#160;&#160;Reported defects</a><br />
              <xsl:for-each select="results/rule">
                 <a href="#{@Name}">&#160;&#160;2.<xsl:value-of select="position()" />&#160;<xsl:value-of select="@Name" /><br />
                 </a>
              </xsl:for-each>
            </p>
					</div>
					<h1><a name="s1">Summary</a></h1>
          <p>
            <a href="http://www.mono-project.com/Gendarme">Gendarme</a> found <xsl:value-of select="count(//rule/target/defect)" /> defects using <xsl:value-of select="count(//rules/rule)" /> rules.
          </p>
					<p>
						<h2>List of assemblies analyzed</h2>
						<ul>
							<xsl:for-each select="files/file">
								<xsl:variable name="file">
                  <xsl:value-of select="@Name" />
                </xsl:variable>
								
								<li><xsl:value-of select="text()" />: <xsl:value-of select="count(//target[@Assembly = $file])" /> defects</li>
							</xsl:for-each>
						</ul>
					</p>
					
					<p>
						<h2>List of rules used</h2>
						
						<xsl:call-template name="print-rules">						
							<xsl:with-param name="type">Assembly</xsl:with-param>
						</xsl:call-template>
						
						<xsl:call-template name="print-rules">
							<xsl:with-param name="type">Type</xsl:with-param>
						</xsl:call-template>
						
						<xsl:call-template name="print-rules">
							<xsl:with-param name="type">Method</xsl:with-param>
						</xsl:call-template>
					</p>
					
					<h1><a name="s2">Reported Defects</a></h1>
					
					<p>
						<xsl:for-each select="results/rule">
							<h3><xsl:value-of select="position()" />&#160;
                <a name="{@Name}" />
								<a href="{@Uri}" target="{@Name}">
									<xsl:value-of select="@Name" />
								</a>
							</h3>

							<b>Problem:</b>
							<p class="problem">
								<xsl:value-of select="problem" />
							</p>

							<b>Found in:</b>
							<xsl:if test="count(target) != 0">
								<xsl:for-each select="target">
                  <p class="found">
									<b>Target:</b>&#160;<xsl:value-of select="@Name" /><br/>
                  <b>Assembly:</b>&#160;<xsl:value-of select="@Assembly" /><br/>
                    <xsl:for-each select="defect">
<!-- FIXME: use different color/style for warnings versus errors -->
                      <span class="found">
                        <br/>
                        <b>Severity:</b>&#160;<xsl:value-of select="@Severity" />&#160;
                        <b>Confidence:</b>&#160;<xsl:value-of select="@Confidence" /><br/>
                        <xsl:if test="@Location != (../@Name)">
                         <b>Location:</b>&#160;<xsl:value-of select="@Location" /><br/>
                        </xsl:if>
                       <xsl:if test="string-length(@Source) &gt; 0">
                          <b>Source:</b>&#160;<xsl:value-of select="@Source" /><br/>
                        </xsl:if>
                        <xsl:if test="string-length(.) &gt; 0">
                          <b>Details:</b>&#160;<xsl:value-of select="." /><br/>
                        </xsl:if>
                      </span>
                    </xsl:for-each>
                  </p>
                </xsl:for-each>
							</xsl:if>

							<b>Solution:</b>
							<p class="solution">
								<xsl:value-of select="solution" />
							</p>							
						</xsl:for-each>
					</p>
				</body>
			</html>
		</xsl:for-each>
	</xsl:template>
</xsl:stylesheet>
