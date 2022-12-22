/*
 * SPDX-License-Identifier: Apache-2.0
 */

package main

type MessageResponse struct {
	Signature string `json:"signature"`
	Counter   int32  `json:"counter"`
	Body      Body   `json:"body"`
}
