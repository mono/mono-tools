<xsl:transform xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

	<!-- default rule -->
	<xsl:template match="*">
		<xsl:copy>
			<xsl:copy-of select="@*" />
			<xsl:apply-templates />
		</xsl:copy>
	</xsl:template>

	<xsl:template match="code">
		<xsl:element name="c"><xsl:apply-templates /></xsl:element>
	</xsl:template>

	<xsl:template match="div">
		<xsl:choose>
			<xsl:when test="@class = 'example'">
				<example><xsl:apply-templates /></example>
			</xsl:when>
			<xsl:when test="@class = 'behavior'">
				<block type="behavior"><xsl:apply-templates /></block>
			</xsl:when>
			<xsl:when test="@class = 'default'">
				<block type="default"><xsl:apply-templates /></block>
			</xsl:when>
			<xsl:when test="@class = 'example-block'">
				<block type="example"><xsl:apply-templates /></block>
			</xsl:when>
			<xsl:when test="@class = 'overrides'">
				<block type="overrides"><xsl:apply-templates /></block>
			</xsl:when>
			<xsl:when test="@class = 'usage'">
				<block type="usage"><xsl:apply-templates /></block>
			</xsl:when>
			<xsl:otherwise>
				<xsl:copy-of select="." />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="table">
		<list type="table">
			<xsl:if test="tr[1]/th">
				<listheader><item>
				<term>
					<xsl:apply-templates select="tr[1]/th[1]" />
				</term>
				<xsl:for-each select="tr[1]/th[position() > 1]">
					<description>
						<xsl:apply-templates />
					</description>
				</xsl:for-each>
				</item></listheader>
			</xsl:if>
			<xsl:for-each select="tr">
				<item>
				<term>
					<xsl:apply-templates select="td[1]" />
				</term>
				<xsl:for-each select="td[position() > 1]">
					<description>
						<xsl:apply-templates />
					</description>
				</xsl:for-each>
				</item>
			</xsl:for-each>
		</list>
	</xsl:template>

</xsl:transform>
